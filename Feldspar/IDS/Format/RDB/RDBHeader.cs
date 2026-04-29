using System.Runtime.InteropServices;
using Feldspar.KTGL;
using Pluto.SourceGen.BitStructGenerator;

namespace Feldspar.IDS.Format.RDB;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct RDBHeader {
	public ResourceHeader Header { get; set; }
	public int Size { get; set; }
	public KTGLPlatform Platform { get; set; }
	public int Count { get; set; }
	public KTID NameDatabaseId { get; set; }
}

public enum RDBStorageType : byte {
	None,
	Zlib, // 32-bit block disk size + zlib data
	Lz4, // 32-bit block disk size + lz4 data
	XorZlib, // XOR(Zlib)
	ChecksumZlib, // 16-bit block disk size + 64-bit checksum + block data
}

public enum RDBLocationType : byte {
	Virtual,
	External,
	Internal
}

[BitStruct(4)]
public partial struct RDBResourceInfo {
	[BitField(16)] public partial ushort Unknown1 { get; set; }
	[BitField(4)] public partial RDBLocationType Location { get; set; }
	[BitField(4)] public partial RDBStorageType Storage { get; set; }
	[BitField(4)] public partial ushort Unknown4 { get; set; }
	[BitField(2)] public partial ushort Unknown5 { get; set; }
	[BitField(2)] public partial ushort Unknown6 { get; set; }
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct RDBIndexHeader {
	public ResourceHeader Header { get; set; }
	public long Size { get; set; }
	public long DiskSize { get; set; }
	public long MemorySize { get; set; }
	public int PropertyValueSize { get; set; }
	public KTID NameId { get; set; }
	public KTID TypeId { get; set; }
	public RDBResourceInfo Info { get; set; }
	public KTID TypeInfoId { get; set; }
	public int PropertyCount { get; set; }
}
