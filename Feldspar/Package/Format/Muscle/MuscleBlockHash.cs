using System.Runtime.CompilerServices;

namespace Feldspar.Package.Format.Muscle;

[InlineArray(16)]
public struct MuscleBlockHash : IEquatable<MuscleBlockHash> {
#pragma warning disable 9020
#pragma warning disable 9022
	public MuscleBlockHash(ReadOnlySpan<byte> key) => key.CopyTo(this);
#pragma warning restore 9020
#pragma warning restore 9022

	private byte Value;

	public byte[] IV {
		get {
			var top = new byte[16];
			var bottom = (stackalloc byte[16]);
			var topIndex = 0;
			var bottomIndex = 0;

			for (var i = 0; i < 16; i++) {
				if ((this[i] & 1) == 1)
					top[topIndex++] = this[i];
				else
					bottom[bottomIndex++] = this[i];
			}

			if (bottomIndex > 0) bottom[..bottomIndex].CopyTo(top.AsSpan(topIndex));

			return top;
		}
	}

	public byte[] Key {
		get {
			var top = new byte[32];
			var bottom = (stackalloc byte[32]);
			var topIndex = 0;
			var bottomIndex = 0;
			var hash = ToString();

			for (var i = 0; i < 32; i++) {
				if (char.IsDigit(hash[i]))
					top[topIndex++] = (byte) hash[i];
				else
					bottom[bottomIndex++] = (byte) hash[i];
			}

			if (bottomIndex > 0) bottom[..bottomIndex].CopyTo(top.AsSpan(topIndex));

			return top;
		}
	}

	public bool Equals(MuscleBlockHash other) => ((ReadOnlySpan<byte>) this).SequenceEqual(other);

	public override bool Equals(object? obj) => obj is MuscleBlockHash other && Equals(other);

	public override int GetHashCode() {
		var hashCode = new HashCode();
		hashCode.AddBytes(this);
		return hashCode.ToHashCode();
	}

	public static bool operator ==(MuscleBlockHash left, MuscleBlockHash right) => left.Equals(right);
	public static bool operator !=(MuscleBlockHash left, MuscleBlockHash right) => !(left == right);

	public override string ToString() => Convert.ToHexStringLower(this);
}
