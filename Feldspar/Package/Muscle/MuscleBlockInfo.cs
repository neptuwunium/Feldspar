using System.Runtime.InteropServices;

namespace Feldspar.Package.Muscle;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 0x28)]
public record struct MuscleBlockInfo {
	public long Offset { get; set; }
	public int MemorySize { get; set; }
	public int CompressedSize { get; set; }
	public int EncryptedSize { get; set; }
	public MuscleBlockHash Hash { get; set; }
	
	public bool IsCompressed => CompressedSize > 0;
	public bool IsEncrypted => EncryptedSize > 0;
}
