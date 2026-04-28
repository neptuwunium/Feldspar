using System.Runtime.InteropServices;

namespace Feldspar.Package.Format.Muscle;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 0x8)]
public record struct MuscleBinTableSize {
	public int Offset { get; set; }
	public int Size { get; set; }
}
