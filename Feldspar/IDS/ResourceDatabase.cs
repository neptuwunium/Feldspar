// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.IO.MemoryMappedFiles;
using Feldspar.IDS.Format;
using Feldspar.IDS.Format.RDB;
using Pluto.IO.Binary;

namespace Feldspar.IDS;

public sealed class ResourceDatabase : IDisposable {
	public ResourceDatabase(string path, ResourceDatabaseManager manager) {
		BasePath = Path.GetDirectoryName(path) ?? throw new InvalidOperationException();
		Manager = manager;
	}

	public ResourceDatabaseManager Manager { get; }
	public RentedArray<RDXInfo> Index { get; }
	public RentedArray<RDBAddressInfo> Addresses { get; }
	public List<Resource> Resources { get; }
	public Dictionary<KTID, MemoryMappedFile> Streams { get; } = [];
	public string BasePath { get; }
	public string ExternalPath { get; }

	public void Dispose() {
		Index.Dispose();
		Addresses.Dispose();

		foreach (var value in Streams.Values) {
			value.Dispose();
		}

		Streams.Clear();

		Manager.Databases.Remove(this);
	}

	public void Remount() {
		foreach (var index in Index) {
			if (!index.CanRemount) {
				continue;
			}

			Mount(index.FDataId, Path.Combine(BasePath, index.ToString()));
		}
	}

	public void Mount(KTID id, string path) {
		if (Streams.TryGetValue(id, out var stream)) {
			stream.Dispose();
		}

		if (!Path.Exists(path)) {
			return;
		}

		Streams[id] = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.ReadWrite);
	}
}
