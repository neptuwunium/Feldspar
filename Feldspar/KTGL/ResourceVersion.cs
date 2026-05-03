// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Feldspar.KTGL;

[InlineArray(4)]
public struct ResourceVersion : IEquatable<ResourceVersion>, IComparable<ResourceVersion> {
#pragma warning disable 9020
#pragma warning disable 9022
	public ResourceVersion(ReadOnlySpan<byte> key) => key[..4].CopyTo(this);
	public ResourceVersion(int version) => Version = version;
#pragma warning restore 9020
#pragma warning restore 9022

	private byte Value;

	// 0000 -> 0
	// 7300 -> 37
	public int Version {
		get =>
			this[0] - 0x30 +
			(this[1] - 0x30) * 10 +
			(this[2] - 0x30) * 100 +
			(this[3] - 0x30) * 1000;
		set {
			this[0] = (byte) (value % 10 + 0x30);
			this[1] = (byte) (value / 10 % 10 + 0x30);
			this[2] = (byte) (value / 100 % 10 + 0x30);
			this[3] = (byte) (value / 1000 % 10 + 0x30);
		}
	}

	public bool Equals(ResourceVersion other) => ((ReadOnlySpan<byte>) this).SequenceEqual(other);
	public int CompareTo(ResourceVersion other) => Version.CompareTo(other.Version);
	public override bool Equals(object? obj) => obj is ResourceVersion other && Equals(other);
	public override int GetHashCode() => MemoryMarshal.Read<int>(this);
	public static bool operator ==(ResourceVersion left, ResourceVersion right) => left.Equals(right);
	public static bool operator !=(ResourceVersion left, ResourceVersion right) => !(left == right);
	public static bool operator >(ResourceVersion left, ResourceVersion right) => left.CompareTo(right) > 0;
	public static bool operator <(ResourceVersion left, ResourceVersion right) => !(left > right);
	public static bool operator >=(ResourceVersion left, ResourceVersion right) => left > right || left == right;
	public static bool operator <=(ResourceVersion left, ResourceVersion right) => left < right || left == right;
	public override string ToString() => Version.ToString();
}
