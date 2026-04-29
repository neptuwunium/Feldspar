// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Feldspar.Package.Format;
using Pluto.IO.Binary;

namespace Feldspar.Package;

public class PackageDataDataSearch {
	public PackageDataDataSearch(BufferBinaryReader reader) {
		Header = reader.Read<PDSHeader>();

		reader.Position = Header.LookupOffset;

		Span<PDSEntry> entries = stackalloc PDSEntry[Header.LookupCount];
		reader.Read(entries);

		for (var index = 0; index < Header.LookupCount; index++) {
			var entry = entries[index];
			reader.Position = entry.LookupIndex * 4 + Header.LookupOffset;
			var key = reader.Read<PDSKey>();

			if (key.WordCount < 5) {
				Span<byte> keySpan = key;
				keySpan[(key.WordCount * 4)..].Clear();
			}

			key.CheckKnown();

			Entries[key] = ReadEntry(entry);
		}

		return;

		(int, uint) ReadEntry(PDSEntry entry) {
			if (entry.EntryIndex == -1 || entry.Flags == 0) {
				return (0, 0);
			}

			reader.Position = Header.ValueOffset + entry.EntryIndex * 4;
			var offset = reader.Read<int>();

			if ((entry.Flags & 0xffffff) != 0) {
				// global type instance
				reader.Position = Header.ValueOffset + 8;
				return (reader.Read<int>() + offset, entry.Flags);
			}

			switch (entry.Flags) {
				case 0x16000000 or 0x80000000 or 0x40000000: // array, class instance, pds instance
					var mask = entry.Flags switch {
						0x16000000 => 0x80000000,
						_ => entry.Flags,
					};
					return (offset, mask);
				case 0x1000000 or 0x2000000 or 0x4000000: // byte, short, int
					return (Header.ValueOffset + entry.EntryIndex * 4 + 4, entry.Flags);
			}

			var adj = reader.Read<uint>();
			// global type instance but funky
			if (adj <= 0xFFFFFF) {
				if ((entry.Flags & 0x10000000) != 0) {
					adj >>= 2;
				}

				var mask = entry.Flags | adj;
				reader.Position = Header.ValueOffset + 8;
				return (reader.Read<int>() + offset, mask);
			}

			return (0, 0);
		}
	}

	public PDSHeader Header { get; set; }
	public Dictionary<PDSKey, (int Offset, uint Mask)> Entries { get; set; } = [];
}
