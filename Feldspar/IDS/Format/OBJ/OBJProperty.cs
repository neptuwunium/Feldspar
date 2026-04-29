using System.Runtime.InteropServices;

namespace Feldspar.IDS.Format.OBJ;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0xC)]
public record struct OBJProperty(OBJPropertyType Type, int Count, KTID Name);

public enum OBJPropertyType : uint {
	Bool,
	Byte,
	Int16,
	UInt16,
	Int32,
	UInt32,
	Int64,
	UInt64,
	Float32,
	Float64,
	Vector4F,
	Matrix4F,
	Vector2F,
	Vector3F,
	None,
}
