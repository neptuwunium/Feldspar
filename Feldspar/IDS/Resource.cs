// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using Feldspar.IDS.Format.OBJ;
using Feldspar.IDS.Format.RDB;
using Pluto.IO.Binary;

namespace Feldspar.IDS;

public class Resource : IDisposable {
	protected Resource(Resource other) {
		Database = other.Database;
		Header = other.Header;
		ObjectData = other.ObjectData;
		AddressInfo = other.AddressInfo;
		Buffer = other.Buffer;
		other.Dispose();
	}

	public Resource(ResourceDatabase database, StreamBinaryReader reader) {
		Database = database;

		var start = reader.Position;
		ParseResourceInfo(reader);

		var addressBuffer = (stackalloc byte[checked((int) Header.DiskSize)]);
		reader.Read(addressBuffer);
		AddressInfo = RDBAddressInfo.TryParse(addressBuffer, out var addressInfo) ? addressInfo : new RDBAddressInfo();

		reader.Position = start + checked((int) Header.Size);
		reader.Align();
	}

	public ResourceDatabase Database { get; }
	public RDBIndexHeader Header { get; set; }
	public RDBAddressInfo AddressInfo { get; set; }
	public ResourceObject ObjectData { get; set; } = ResourceObject.Empty;
	public RentedArray<byte> Buffer { get; set; } = RentedArray<byte>.Empty;
	public bool IsLoaded => Buffer.Length > 0;

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public void ParseResourceInfo(StreamBinaryReader reader) {
		Header = reader.Read<RDBIndexHeader>();

		if (Header.PropertyCount > 0) {
			Debug.Assert(Header.PropertyValueSize > 0, "properties provided without data");

			if (!ObjectData.IsEmpty) {
				ObjectData.Dispose();
			}

			var properties = (stackalloc OBJProperty[Header.PropertyCount]);
			reader.Read(properties);

			ObjectData = new ResourceObject(properties, reader.Read<byte>(Header.PropertyValueSize));
		} else {
			Debug.Assert(Header.PropertyValueSize == 0, "properties data provided without properties");
			reader.Position += Header.PropertyValueSize;
		}
	}

	public virtual void Create() {
		if (IsLoaded) {
			return;
		}

		Database.Read(this);
	}

	public virtual void Destroy() {
		if (!IsLoaded) {
			return;
		}

		Buffer.Dispose();
		Buffer = RentedArray<byte>.Empty;
	}

	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			Destroy();
			ObjectData.Dispose();
		}
	}
}
