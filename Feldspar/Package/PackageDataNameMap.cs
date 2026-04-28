// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using Feldspar.Package.Format;
using Pluto;
using Pluto.IO.Binary;

namespace Feldspar.Package;

public sealed class PackageDataNameMap : IDisposable {
	public PackageDataNameMap(BufferBinaryReader reader) {
		var header = reader.Read<DINHeader>();
		NamePartCount = header.NamePartCount;

		Span<DINEntry> entries = stackalloc DINEntry[header.LookupCount];
		reader.Read(entries);

		var stringStart = reader.Position;
		Span<ushort> stringInfo = stackalloc ushort[header.LookupCount * 2];
		reader.Read(stringInfo);

		Names.Clear();
		Names.EnsureCapacity(header.LookupCount);
		for (var index = 0; index < header.LookupCount * 2; index += 2) {
			var suffixOffset = stringInfo[index];
			var prefixOffset = stringInfo[index + 1];

			Names.Add(ReadString(prefixOffset) + ReadString(suffixOffset));
		}

		return;

		string ReadString(ushort offset) {
			reader.Position = stringStart + offset;
			var length = reader.Read<byte>();
			return reader.ReadCString<byte>(Encoding.ASCII, length, true);
		}
	}

	public int NamePartCount { get; set; }
	public List<string> Names { get; private set; } = ObjectPool<List<string>>.Rent();

	public void Dispose() {
		ObjectPool<List<string>>.Return(Names);
		Names = null!;
	}
}
