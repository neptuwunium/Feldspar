using System.Runtime.InteropServices;

namespace Feldspar.Package.Muscle;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 0x10)]
public record struct MuscleBinHeader {
	public int Version { get; set; }
	public int TypeIdOffset { get; set; }
	public int Count { get; set; }
}
