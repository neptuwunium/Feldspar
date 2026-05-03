// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Text;
using Charon.Compression;
using Feldspar.IDS.Format;
using Feldspar.IDS.Format.RDB;
using Feldspar.KTGL;
using Pluto;
using Pluto.IO.Binary;
using Serilog;

namespace Feldspar.IDS;

public sealed class ResourceDatabase : IDisposable {
	// todo: move this to a file
	private static readonly string[] KTGL_EXTRA_MOUNTS_BASE = ["@../../shader_@", "../../shader"];

	public ResourceDatabase(string path, ResourceDatabaseManager manager) {
		BasePath = Path.GetDirectoryName(path) ?? throw new InvalidOperationException();
		Manager = manager;
		Name = Path.GetFileNameWithoutExtension(path);

		Log.Information("[rdb] reading ResourceDatabase {Name}", Name);

		DatabaseStream = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
		using var reader = new StreamBinaryReader(DatabaseStream.CreateViewStream(0, 0, MemoryMappedFileAccess.Read));
		var header = reader.Read<RDBHeader>();
		ExternalPath = reader.ReadCString<byte>(Encoding.UTF8, header.Size - Unsafe.SizeOf<RDBHeader>());

		foreach (var tmp in KTGL_EXTRA_MOUNTS_BASE) {
			var testPath = tmp;
			if (testPath[0] == '@') {
				testPath = testPath[1..].Replace("@", header.Platform.EngineName, StringComparison.Ordinal);
			}

			var target = Path.Combine(BasePath, ExternalPath, testPath);
			if (Directory.Exists(target)) {
				ExtraExternalPaths.Add(Path.Combine(ExternalPath, testPath));
			}
		}

		var rdxPath = Path.ChangeExtension(path, "rdx");
		if (File.Exists(rdxPath)) {
			using var rdxStream = new FileStream(rdxPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var rdxReader = new StreamBinaryReader(rdxStream);

			Index = rdxReader.Read<RDXInfo>((int) (rdxStream.Length / Unsafe.SizeOf<RDXInfo>()));
		}

		reader.Position = header.Size;

		Resources = ObjectPool<Dictionary<KTID, Resource>>.Rent();
		Resources.Clear();
		Resources.EnsureCapacity(header.Count);

		for (var index = 0; index < header.Count; index++) {
			var resource = new Resource(this, reader);
			if (resource.AddressInfo.Index.Index < 0xFFFF) {
				var realIndex = resource.AddressInfo.Index.Index; // Index.BinarySearch(resource.AddressInfo.Index, RDXInfo.IndexComparer);
				resource.AddressInfo = resource.AddressInfo with {
					Index = Index[realIndex],
				};

				Debug.Assert(resource.AddressInfo.Index.Index == realIndex);
			}

			// this is what Nioh3 does. sub_141563740 in demo.
			if (resource.Header.TypeId == 0x7bcd279f && resource.Header.MemorySize == 0) {
				// G1SFile
				var info = resource.Header.Info;
				info.Location = RDBLocationType.External;
				resource.Header = resource.Header with {
					Info = info,
				};

				// you know this is mildly frustrating since there's literally a mounting system
				// just add mount 0 to be ../../shader_dx12????
			}

			Resources.Add(resource.Header.NameId, resource);
		}

		Remount(true);
	}

	public ResourceDatabaseManager Manager { get; }
	public RentedArray<RDXInfo> Index { get; } = RentedArray<RDXInfo>.Empty;
	public Dictionary<KTID, Resource> Resources { get; }
	public MemoryMappedFile DatabaseStream { get; }
	public Dictionary<KTID, MemoryMappedFile?> Streams { get; } = [];
	public string BasePath { get; }
	public string ExternalPath { get; }
	public List<string> ExtraExternalPaths { get; } = [];
	public KTID Name { get; }

	public void Dispose() {
		Index.Dispose();

		foreach (var value in Streams.Values) {
			value?.Dispose();
		}

		Streams.Clear();

		foreach (var value in Resources.Values) {
			value.Dispose();
		}

		ObjectPool<Dictionary<KTID, Resource>>.Return(Resources);
		Resources.Clear();

		Manager.Databases.Remove(Name);
	}

	public void Remount(bool force = false) {
		var myPath = Path.Combine(BasePath, Name + ".rdb.bin");
		var loosePaths = new List<string> { Path.Combine(BasePath, ExternalPath) };
		loosePaths.AddRange(ExtraExternalPaths.Select(path => Path.Combine(BasePath, path)));

		foreach (var resource in Resources.Values) {
			var address = resource.AddressInfo;
			if (!address.IsValid) {
				continue;
			}

			// path resolution for 3 generations of file resolution logic.
			// muscle variant: no unique paths, everything is handled with bin suffixes
			// package variant: path is stored inside the address info
			// rdx variant: data is referencing rdx indices
			if (address.Index.IsValid) {
				// rdx variant, it can potentially remount.
				if (!force && !address.Index.CanRemount) {
					continue;
				}

				if ((address.IndexFlags & RDXFlags.ExternalFile) == 0) {
					Mount(address.Index.FDataId, Path.Combine(BasePath, address.Index.ToString()), force);
				} else {
					MountExternal(resource, address.Index.CanRemount ? Path.Combine(BasePath, Path.GetDirectoryName(address.Index.ToString())!) : BasePath);
				}
			} else {
				// muscle OR package variant
				// only rdx can remount to different mount points and languages
				if (!force) {
					continue;
				}

				if (resource.Header.Info.Location != RDBLocationType.External) {
					var targetPath = myPath;

					// package variant
					if (!string.IsNullOrEmpty(address.ExternalPath)) {
						targetPath = address.ExternalPath;
					}

					targetPath += address.Ext;
					Mount(KTID.CreateKTID(Path.GetFileName(targetPath)), targetPath, force);
				} else {
					// muscle variant
					Debug.Assert(string.IsNullOrEmpty(address.ExternalPath));
					MountExternal(resource, BasePath);
				}
			}
		}

		return;

		bool TryMountExternal(KTID nameId, KTID targetId, string basePath) {
			var name = $"0x{nameId.Value:x08}.file";
			var shortId = (nameId.Value & 0xff).ToString("x2");
			foreach (var looseDir in loosePaths) {
				if (Mount(targetId, Path.Combine(basePath, looseDir, name), force)) {
					return true;
				}

				if (Mount(targetId, Path.Combine(basePath, looseDir, shortId, name), force)) {
					return true;
				}
			}

			return false;
		}

		void MountExternal(Resource resource, string basePath) {
			if (TryMountExternal(resource.Header.NameId, resource.Header.NameId, basePath)) {
				return;
			}

			if (resource.Header.MemorySize == 0) {
				// this is what Nioh3 does. sub_141563740 in demo.
				// G1SFile
				var isG1S = resource.Header.TypeId == 0x7bcd279f;
				if (MountRemappedResource(resource, basePath, isG1S ? ResourceRemapping.Shader : ResourceRemapping.Resource)) {
					return;
				}

				if (!isG1S) {
					return;
				}

				foreach (var looseDir in loosePaths) {
					if (Mount(resource.Header.NameId, Path.Combine(basePath, looseDir, "PB2Unknown.file"), force)) {
						break;
					}
				}
			}
		}

		bool MountRemappedResource(Resource resource, string basePath, Dictionary<KTID, KTID> remap) {
			if (!remap.TryGetValue(resource.Header.NameId, out var nameId)) {
				return false;
			}

			if (nameId == resource.Header.NameId || nameId == default) {
				return false;
			}

			return TryMountExternal(nameId, resource.Header.NameId, basePath);
		}
	}

	public KTID GetResourcePackage(Resource resource) {
		var address = resource.AddressInfo;
		if (!address.IsValid) {
			return default;
		}

		if (address.Index.IsValid) {
			return (address.IndexFlags & RDXFlags.ExternalFile) != 0 ? resource.Header.NameId : address.Index.FDataId;
		}

		if (resource.Header.Info.Location != RDBLocationType.External) {
			var targetPath = Path.Combine(BasePath, Name + ".rdb.bin");

			if (!string.IsNullOrEmpty(address.ExternalPath)) {
				targetPath = address.ExternalPath;
			}

			targetPath += address.Ext;

			return KTID.CreateKTID(Path.GetFileName(targetPath));
		}

		return resource.Header.NameId;
	}

	public bool Mount(KTID id, string path, bool isMounting = false) {
		if (Streams.TryGetValue(id, out var stream)) {
			if (isMounting) {
				return true;
			}

			stream?.Dispose();
		}

		if (!Path.Exists(path)) {
			Log.Information("[rdb] cannot {Type} {Path} as it does not exist", isMounting ? "mount" : "remount", Path.GetRelativePath(BasePath, path));
			Streams[id] = null;
			return false;
		}

		Log.Information("[rdb] {Type} {Path}", isMounting ? "mounting" : "remounting", Path.GetRelativePath(BasePath, path));
		Streams[id] = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
		return true;
	}

	public bool LoadResource(Resource resource) {
		const int BLOCK_SIZE = 0x4000;

		if (resource.IsLoaded) {
			return true;
		}

		if (Streams.GetValueOrDefault(GetResourcePackage(resource)) is not { } stream) {
			return false;
		}

		var offset = resource.AddressInfo.Offset;
		if (resource.AddressInfo.Index.IsValid && (resource.AddressInfo.IndexFlags & RDXFlags.ExternalFile) != 0) {
			offset = 0;
		}

		// re-read header, this is necessary because nioh3 gaslights the system
		int innerOffset;
		using (var tempReader = new StreamBinaryReader(stream.CreateViewStream(offset, resource.AddressInfo.Length, MemoryMappedFileAccess.Read))) {
			resource.ReadResourceInfo(tempReader);
			innerOffset = tempReader.Position;
		}

		if (resource.Header.MemorySize == 0) {
			return true;
		}

		using var reader = new StreamBinaryReader(stream.CreateViewStream(offset + innerOffset, resource.Header.DiskSize, MemoryMappedFileAccess.Read));

		var buffer = new RentedArray<byte>(checked((int) resource.Header.MemorySize));
		try {
			if (resource.Header.Info.Storage == RDBStorageType.None) {
				reader.Read(buffer.Span);
				resource.Buffer = buffer;
				return true;
			}

			using var scratch = new RentedArray<byte>(BLOCK_SIZE);
			var output = buffer.Memory;
			var bufferOffset = 0;

			var comType = resource.Header.Info.Storage switch {
				RDBStorageType.Lz4 => CompressionType.LZ4,
				_ => CompressionType.Zlib,
			};

			while (bufferOffset < resource.Header.MemorySize) {
				var remainingChunk = checked((int) Math.Min(BLOCK_SIZE, resource.Header.MemorySize - bufferOffset));
				var targetBlock = output.Slice(bufferOffset, remainingChunk);

				switch (resource.Header.Info.Storage) {
					case RDBStorageType.Zlib:
					case RDBStorageType.Lz4: {
						var compressedSize = reader.Read<int>();
						if (compressedSize == 0) {
							break;
						}

						Debug.Assert(compressedSize is > 0 and < BLOCK_SIZE);

						reader.Read(scratch.Span[..compressedSize]);
						bufferOffset += CompressionHelper.Decompress(comType, scratch.Memory[..compressedSize], targetBlock);
						continue;
					}
					case RDBStorageType.ChecksumZlib: {
						var compressedSize = reader.Read<ushort>();
						if (compressedSize == 0) {
							break;
						}

						reader.Position += 8; // skip checksums

						Debug.Assert(compressedSize is > 0 and < BLOCK_SIZE);

						reader.Read(scratch.Span[..compressedSize]);
						bufferOffset += CompressionHelper.Decompress(comType, scratch.Memory[..compressedSize], targetBlock);
						continue;
					}
					case RDBStorageType.XorZlib: throw new NotSupportedException();
					case RDBStorageType.None: throw new UnreachableException();
					default: throw new NotSupportedException();
				}
			}

			if (bufferOffset >= resource.Header.MemorySize) {
				resource.Buffer = buffer;
				return true;
			}
		} catch {
			buffer.Dispose();
			throw;
		}

		buffer.Dispose();
		return false;
	}

	public bool UnloadResource(Resource resource) {
		if (!resource.IsLoaded) {
			return true;
		}

		resource.Buffer.Dispose();
		resource.Buffer = RentedArray<byte>.Empty;

		using var reader = new StreamBinaryReader(DatabaseStream.CreateViewStream(resource.AddressInfo.RDBPosition, 0, MemoryMappedFileAccess.Read));
		resource.ReadResourceInfo(reader);

		return true;
	}
}
