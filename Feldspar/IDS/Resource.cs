// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Feldspar.IDS.Format;
using Feldspar.IDS.Format.OBJ;
using Feldspar.IDS.Format.RDB;
using Feldspar.KTGL;
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
		var info = RDBAddressInfo.TryParse(addressBuffer, out var addressInfo) ? addressInfo : RDBAddressInfo.Default;
		info.RDBPosition = start;
		AddressInfo = info;

		reader.Position = start + checked((int) Header.Size);
		reader.Align();
	}

	public Resource(KTID typeId, KTID nameId, RentedArray<byte> buffer, KTID resourceId = default) {
		Header = new RDBIndexHeader {
			Header = new ResourceHeader(ResourceMagic.ResourceDatabaseIndex, new ResourceVersion("0000"u8)),
			Size = buffer.Length + Unsafe.SizeOf<RDBIndexHeader>(),
			DiskSize = buffer.Length,
			MemorySize = buffer.Length,
			Info = RDBResourceInfo.Virtual,
			TypeId = typeId,
			NameId = nameId,
			ResourceId = resourceId,
		};
		AddressInfo = RDBAddressInfo.Default;
		Buffer = buffer;
	}

	public ResourceDatabase? Database { get; }
	public RDBIndexHeader Header { get; set; }
	public RDBAddressInfo AddressInfo { get; set; }
	public ResourceObjectData ObjectData { get; set; } = ResourceObjectData.Empty;
	public ResourceObject? Object { get; set; }
	public RentedArray<byte> Buffer { get; set; } = RentedArray<byte>.Empty;
	public bool IsLoaded => Buffer.Length > 0 || Header.MemorySize == 0;
	public bool IsVirtual => Header.Info.IsVirtual;

	public void Dispose() {
		Destroy();
		ObjectData.Dispose();
		Object?.Dispose();

		if (IsVirtual) {
			Buffer.Dispose();
			Buffer = RentedArray<byte>.Empty;
		}
	}

	public void ReadResourceInfo(StreamBinaryReader reader) {
		if (IsVirtual) {
			return;
		}

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

	public bool Load() => IsVirtual || Database == null || IsLoaded || Database.LoadResource(this);
	public bool Unload() => IsVirtual || Database == null || IsLoaded && Database.UnloadResource(this);
}
