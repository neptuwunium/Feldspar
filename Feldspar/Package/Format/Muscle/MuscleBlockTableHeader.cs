using System.Runtime.InteropServices;
using Feldspar.Package.Format.IDSOBJ;

namespace Feldspar.Package.Format.Muscle;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 0x10)]
public record struct MuscleBlockTableHeader {
	public static KTID Id { get; } = 0x0A9BFFB9u;

	public int Count { get; set; }
	public int LookupOffset { get; set; }
	public int LookupCount { get; set; }
}
