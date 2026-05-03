// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using Feldspar.IDS.Format.OBJ;
using Feldspar.IDS.Format.RDB;
using Pluto.IO.Binary;

namespace Feldspar.IDS;

public sealed class Resource : IDisposable {
	private static readonly Type[] OBJECT_READER_CONSTRUCTOR_ARGS = [typeof(Resource), typeof(BufferBinaryReader)];
	private static readonly Type[] OBJECT_CONSTRUCTOR_ARGS = [typeof(Resource)];

	public Resource(ResourceDatabase database, StreamBinaryReader reader) {
		Database = database;

		var start = reader.Position;
		ReadResourceInfo(reader);

		var addressBuffer = (stackalloc byte[checked((int) Header.DiskSize)]);
		reader.Read(addressBuffer);
		var info = RDBAddressInfo.TryParse(addressBuffer, out var addressInfo) ? addressInfo : new RDBAddressInfo();
		info.RDBPosition = start;
		AddressInfo = info;

		reader.Position = start + checked((int) Header.Size);
		reader.Align();
	}

	public ResourceDatabase Database { get; }
	public RDBIndexHeader Header { get; set; }
	public RDBAddressInfo AddressInfo { get; set; }
	public ResourceObjectData ObjectData { get; set; } = ResourceObjectData.Empty;
	public ResourceObject? Object { get; set; }
	public RentedArray<byte> Buffer { get; set; } = RentedArray<byte>.Empty;
	public bool IsLoaded => Buffer.Length > 0 || Header.MemorySize == 0;

	public void Dispose() {
		Destroy();
		ObjectData.Dispose();
		Object?.Dispose();
	}

	public void ReadResourceInfo(StreamBinaryReader reader) {
		Header = reader.Read<RDBIndexHeader>();

		if (Header.ParamHeaderCount > 0) {
			Debug.Assert(Header.ParamDataSize > 0, "properties provided without data");

			if (!ObjectData.IsEmpty) {
				ObjectData.Dispose();
			}

			var parameters = (stackalloc OBJParam[Header.ParamHeaderCount]);
			reader.Read(parameters);

			ObjectData = new ResourceObjectData(parameters, reader.Read<byte>(Header.ParamDataSize));
		} else {
			Debug.Assert(Header.ParamDataSize == 0, "properties data provided without properties");
			reader.Position += Header.ParamDataSize;
		}
	}

	public bool Create(bool recreate = false) {
		if (!Load()) {
			return false;
		}

		if (Object != null) {
			if (!recreate) {
				return true;
			}

			Object.Dispose();
			Object = null;
		}

		if (!ObjectTypeRegistry.Types.TryGetValue(Header.TypeId, out var type)) {
			return false;
		}

		if (Buffer.Length > 0 && type.GetConstructor(OBJECT_READER_CONSTRUCTOR_ARGS) is { } readerConstructor) {
			Object = (ResourceObject) readerConstructor.Invoke(null, [this, new ArrayPoolBinaryReader(Buffer, true)])!;
		} else if (type.GetConstructor(OBJECT_CONSTRUCTOR_ARGS) is { } constructor) {
			Object = (ResourceObject) constructor.Invoke(null, [this])!;
		}

		return Object != null;
	}

	public bool Destroy() {
		Object?.Dispose();
		Object = null;
		return Unload();
	}

	public bool Load() => IsLoaded || Database.LoadResource(this);

	public bool Unload() => IsLoaded && Database.UnloadResource(this);
}
