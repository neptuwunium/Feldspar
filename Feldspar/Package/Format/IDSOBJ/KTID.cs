using System.Text;
using Pluto.SourceGen.TransparentStructGenerator;

namespace Feldspar.Package.Format.IDSOBJ;

[TransparentStruct<uint>]
public partial struct KTID {
	public KTID(string text) {
		this = CreateKTID(text);
	}
	
	public KTID(ReadOnlySpan<byte> text) {
		if (text.Length == 0) {
			Value = 0;
			return;
		}
		
		this = CreateKTID(text, text[0] * 0x1f);
	}

	public KTID(ReadOnlySpan<byte> text, int hash) => this = CreateKTID(text, hash);

	public static KTID CreateKTID(string text) {
		if (string.IsNullOrEmpty(text)) {
			return default;
		}

		var stack = (stackalloc byte[Encoding.UTF8.GetByteCount(text)]);
		var n = Encoding.UTF8.GetBytes(text, stack);

		return CreateKTID(stack[..n], stack[0] * 0x1f);
	}

	public static KTID CreateKTID(ReadOnlySpan<byte> text, int hash) {
		var inc = 0x1f;
		unchecked {
			foreach (var ch in text) {
				var state = inc;
				inc *= 0x1f;
				hash += 0x1f * state * (sbyte) ch;
			}
			
			return (uint) hash;
		}
	}

	public static KTID CreateHash(string text) {
		if (string.IsNullOrEmpty(text)) {
			return default;
		}

		var stack = (stackalloc byte[Encoding.UTF8.GetByteCount(text)]);
		var n = Encoding.UTF8.GetBytes(text, stack);

		return CreateHash(stack[..n], 0);
	}

	// boost::hash_combine, i think
	public static KTID CreateHash(ReadOnlySpan<byte> text, uint hash) {
		foreach (var ch in text) {
			hash ^= (hash << 6) + (hash >> 2) - 0x61c88647 + ch;
		}

		return hash;
	}

	public override string ToString() => KTIDRegistry.Lookup.TryGetValue(Value, out var text) ? text : Value.ToString("x8");
	public static implicit operator KTID(string value) => new(value);
}

public static class KTIDRegistry {
	public static Dictionary<uint, string> Lookup { get; } = [];
}
