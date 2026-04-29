// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Text;
using Feldspar.IDS.Format;
using Feldspar.IDS.Format.RDB;
using Pluto;
using Pluto.IO.Binary;
using Serilog;

namespace Feldspar.IDS;

public sealed class ResourceDatabase : IDisposable {
	public ResourceDatabase(string path, ResourceDatabaseManager manager) {
		BasePath = Path.GetDirectoryName(path) ?? throw new InvalidOperationException();
		Manager = manager;
		Name = Path.GetFileNameWithoutExtension(path);

		Log.Information("[rdb] reading ResourceDatabase {Name}", Name);

		using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using var reader = new StreamBinaryReader(stream);
		var header = reader.Read<RDBHeader>();
		ExternalPath = reader.ReadCString<byte>(Encoding.UTF8, header.Size - Unsafe.SizeOf<RDBHeader>());

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

			Resources.Add(resource.Header.NameId, resource);
		}

		Remount(true);
	}

	public ResourceDatabaseManager Manager { get; }
	public RentedArray<RDXInfo> Index { get; } = RentedArray<RDXInfo>.Empty;
	public Dictionary<KTID, Resource> Resources { get; }
	public Dictionary<KTID, MemoryMappedFile> Streams { get; } = [];
	public string BasePath { get; }
	public string ExternalPath { get; }
	public KTID Name { get; }

	public void Dispose() {
		Index.Dispose();

		foreach (var value in Streams.Values) {
			value.Dispose();
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
		var looseDir = Path.Combine(BasePath, ExternalPath);
		var myPath = Path.Combine(BasePath, Name + ".rdb.bin");

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

				if ((address.IndexFlags & RDXFlags.ExternalFile) != 0) {
					if (address.Index.CanRemount) {
						Mount(resource.Header.NameId, Path.Combine(BasePath, Path.GetDirectoryName(address.Index.ToString())!, ExternalPath, $"0x{resource.Header.NameId.Value:x08}.file"), force);
					} else {
						Mount(resource.Header.NameId, Path.Combine(BasePath, ExternalPath, $"0x{resource.Header.NameId.Value:x08}.file"), force);
					}
				} else {
					Mount(address.Index.FDataId, Path.Combine(BasePath, address.Index.ToString()), force);
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
					Mount(resource.Header.NameId, Path.Combine(looseDir, $"0x{resource.Header.NameId.Value:x08}.file"), force);
				}
			}
		}
	}

	public void Mount(KTID id, string path, bool isMounting = false) {
		if (Streams.TryGetValue(id, out var stream)) {
			if (isMounting) {
				return;
			}

			stream.Dispose();
		}

		if (!Path.Exists(path)) {
			return;
		}

		Log.Information("[rdb] {Type} {Path}", isMounting ? "mounting" : "remounting", Path.GetRelativePath(BasePath, path));
		Streams[id] = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
	}

	public void Read(Resource resource) => throw new NotImplementedException();
}
