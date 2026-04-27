using Pluto.SourceGen.TransparentStructGenerator;

namespace Feldspar.Package.IDSOBJ;

[TransparentStruct<uint>]
public partial struct KTID {
	public override string ToString() {
		return Value.ToString("x8");
	}
}
