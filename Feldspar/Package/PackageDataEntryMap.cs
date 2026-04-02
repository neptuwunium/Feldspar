// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Pluto.IO.Binary;

namespace Feldspar.Package;

public class PackageDataEntryMap : PackageDataDataSearch {
	public PackageDataEntryMap(BufferBinaryReader reader) : base(reader) {
		if (Entries.TryGetValue(PDSKeyRegistry.Count, out var offset) && offset.Offset > 0) {
			reader.Position = offset.Offset;
			var count = reader.Read<int>();
			if (count > 0)
				for (var index = 0; index < count; ++index) {
					reader.Position += 4; // 1
					PartResourceIndices.Add(reader.Read<int>());
					reader.Position += 0x18; // 0, 0, 0, 0, 1, -1
				}
		}

		if (Entries.TryGetValue(PDSKeyRegistry.TextureResourceId, out offset) && offset.Offset > 0) {
			reader.Position = offset.Offset;
			TextureResourceId = reader.Read<int>();
		}

		if (Entries.TryGetValue(PDSKeyRegistry.MaterialResourceId, out offset) && offset.Offset > 0) {
			reader.Position = offset.Offset;
			MaterialResourceId = reader.Read<int>();
		}

		if (Entries.TryGetValue(PDSKeyRegistry.ModelResourceId, out offset) && offset.Offset > 0) {
			reader.Position = offset.Offset;
			ModelResourceId = reader.Read<int>();
		}
	}

	public int? TextureResourceId { get; set; }
	public int? MaterialResourceId { get; set; }
	public int? ModelResourceId { get; set; }
	public List<int> PartResourceIndices { get; set; } = [];
}
