// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Text;
using Charon.Compression;
using Feldspar.Package.Format.Decade;
using Feldspar.Security;
using Pluto.Extensions;
using Pluto.IO.Binary;

namespace Feldspar.Package.Decade;

public class DecadeTable {
	public DecadeTable(string dataDir, RentedArray<byte> data, bool leaveOpen = false) {
		DataDir = dataDir;

		using var stream = new ArrayPoolBinaryReader(data, leaveOpen);
		var length = stream.Read<int>();
		DecadeCipher.Crypt(data.Span[4..], length, DecadeKeyRing.Decade);
		using var decompressed = Decompress(stream, length);
		using var tableReader = new ArrayPoolBinaryReader(decompressed, leaveOpen);

		var header = tableReader.Read<DecadeTableHeader>();
		Files.EnsureCapacity(header.Count);

		using var entries = tableReader.Read<DecadeFileInfo>(header.Count);
		var decompressedSpan = decompressed.Span[header.Offset..];
		foreach (var entry in entries) {
			var blockName = decompressedSpan[entry.BlockNameOffset..].ReadString(Encoding.ASCII) ?? throw new UnreachableException();
			var pathName = decompressedSpan[entry.PathNameOffset..].ReadString(Encoding.ASCII) ?? throw new UnreachableException();
			Files[pathName] = (entry, blockName);
		}
	}

	public Dictionary<string, (DecadeFileInfo Info, string BlockName)> Files { get; } = new(StringComparer.OrdinalIgnoreCase);

	public string DataDir { get; }

	public RentedArray<byte> OpenFile(string path) {
		if (!Files.TryGetValue(path, out var lookup)) {
			return RentedArray<byte>.Empty;
		}

		var blockPath = Path.Combine(DataDir, lookup.BlockName);
		if (!File.Exists(blockPath)) {
			return RentedArray<byte>.Empty;
		}

		var block = RentedArray<byte>.FromFile(blockPath);
		try {
			if ((lookup.Info.Flags & DecadeFlags.Encrypted) != 0) {
				DecadeCipher.Crypt(block.Span, lookup.Info.MemorySize, DecadeKeyRing.Decade);
			}

			File.WriteAllBytes("test.bin", block.Span);

			if ((lookup.Info.Flags & DecadeFlags.Compressed) == 0) {
				return block;
			}

			try {
				return Decompress(block, lookup.Info.MemorySize);
			} finally {
				block.Dispose();
			}
		} catch {
			block.Dispose();
			throw;
		}
	}

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
