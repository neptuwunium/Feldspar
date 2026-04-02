// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Charon.Compression;
using Pluto.Extensions;
using Pluto.IO.Binary;

namespace Feldspar.Compression;

public static class BlockCompression {
	public static RentedArray<byte> Decompress(BufferBinaryReader reader, int length) {
		var result = new RentedArray<byte>(length);

		var baseOffset = reader.Position;
		var decPos = 0;
		var decMem = result.Memory;
		var decSpan = result.Span;

		while (decPos < length) {
			var size = reader.Read<int>();
			var isCompressed = ((size >> 15) & 1) == 1;
			size &= 0x7fff;

			var start = reader.Position - baseOffset;

			if (!isCompressed) {
				reader.ReadBytes(decSpan.Slice(decPos, Math.Min(size, reader.Length - reader.Position)));
				decPos += size;
			} else {
				using var shared = reader.ReadSharedBytes(size);
				decPos += CompressionHelper.Decompress(CompressionType.Zlib, shared.Memory, decMem.Slice(decPos, Math.Min(0x4000, length - decPos)));
			}

			reader.Position = (start + size).Align(0x10) + baseOffset;
		}

		return result;
	}

	public static RentedArray<byte> Decompress(RentedArray<byte> data, int length) {
		using var reader = new ArrayPoolBinaryReader(data);
		return Decompress(reader, length);
	}
}
