using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using Charon.Compression;
using Feldspar.Package.Format.Muscle;
using Feldspar.Package.Format.IDSOBJ;
using Pluto.IO.Binary;

namespace Feldspar.Package.Muscle;

public sealed class MuscleBinFile : IDisposable {
	public MuscleBinFile(string path) : this(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }

	public MuscleBinFile(FileStream stream) {
		using var reader = new StreamBinaryReader(stream, true);
		var header = reader.Read<MuscleBinHeader>();
		Debug.Assert(header.Count == 1);

		var tableSizes = (stackalloc MuscleBinTableSize[header.Count]);
		reader.Read(tableSizes);

		reader.Position = header.TypeIdOffset;
		var typeIds = (stackalloc KTID[header.Count]);
		reader.Read(typeIds);

		Debug.Assert(typeIds[0] == MuscleBlockTableHeader.Id);

		var baseOffset = tableSizes[0].Offset;
		var dataOffset = baseOffset + tableSizes[0].Size;
		reader.Position = baseOffset;

		var tableHeader = reader.Read<MuscleBlockTableHeader>();
		var ptrOffsets = (stackalloc int[tableHeader.Count]);
		reader.Read(ptrOffsets);

		reader.Position = tableHeader.LookupOffset + baseOffset;
		var lookupEntries = (stackalloc MuscleBlockTableRecord[tableHeader.LookupCount]);
		reader.Read(lookupEntries);

		foreach (var lookupEntry in lookupEntries) {
			reader.Position = ptrOffsets[lookupEntry.Index] + baseOffset;

			var info = reader.Read<MuscleBlockInfo>();
			info.Offset += dataOffset;
			Files[lookupEntry.Name] = info;
		}

		stream.Position = 0;
		MemoryMappedFile = MemoryMappedFile.CreateFromFile(stream, null, 0, MemoryMappedFileAccess.Read,  HandleInheritability.None, false);
	}

	public RentedArray<byte> OpenFile(KTID id) {
		if (!Files.TryGetValue(id, out var info) || info.MemorySize == 0) {
			return RentedArray<byte>.Empty;
		}

		var result = new RentedArray<byte>(info.MemorySize);

		var readSize = info.IsEncrypted ? info.EncryptedSize : info.IsCompressed ? info.CompressedSize : info.MemorySize;

		using var view = MemoryMappedFile.CreateViewStream(info.Offset, readSize, MemoryMappedFileAccess.Read);

		if (!info.IsEncrypted && !info.IsCompressed) {
			view.ReadExactly(result.Span);
			return result;
		}

		using var work = new RentedArray<byte>(info.MemorySize);
		if (info.IsEncrypted) {
			view.ReadExactly(work.Span[..info.EncryptedSize]);
			using var aes = Aes.Create();
			aes.KeySize = 256;
			aes.Key = info.Hash.Key;
			aes.DecryptCbc(work.Span[..info.EncryptedSize], info.Hash.IV, work.Span[..info.EncryptedSize]);
		} else {
			view.ReadExactly(work.Span[..info.CompressedSize]);
		}

		if (!info.IsCompressed) {
			work.Span[..info.MemorySize].CopyTo(result.Span);
		} else {
			CompressionHelper.Decompress(CompressionType.Zlib, work.Memory[..info.MemorySize], result.Memory);
		}

		return result;
	}

	public Dictionary<KTID, MuscleBlockInfo> Files { get; } = [];
	public MemoryMappedFile MemoryMappedFile { get; }
	public void Dispose() => MemoryMappedFile.Dispose();
}
