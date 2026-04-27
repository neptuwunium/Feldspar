using System.Runtime.InteropServices;
using Feldspar.Package.IDSOBJ;

namespace Feldspar.Package.Muscle;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 0x8)]
public record struct MuscleBlockTableRecord {
	public KTID Name { get; set; }
	public int Index { get; set; }
}
