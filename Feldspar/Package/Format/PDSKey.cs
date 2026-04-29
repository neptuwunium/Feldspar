// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Feldspar.Package.Format;

[InlineArray(20)]
public struct PDSKey : IEquatable<PDSKey> {
#pragma warning disable 9020
#pragma warning disable 9022
	public PDSKey(ReadOnlySpan<byte> key) => key[..(key.Length > 20 ? 20 : key.Length)].CopyTo(this);

	public PDSKey(string key) => Convert.FromHexString(key, this, out _, out _);
#pragma warning restore 9022
#pragma warning restore 9020

	private byte Value;

	public bool Equals(PDSKey other) {
		var left = WordCount < 5 ? ((ReadOnlySpan<byte>) this)[..(WordCount * 4)] : this;
		var right = other.WordCount < 5 ? ((ReadOnlySpan<byte>) this)[..(other.WordCount * 4)] : this;
		return left.SequenceEqual(right);
	}

	public override bool Equals(object? obj) => obj is PDSKey other && Equals(other);

	public int WordCount => (int) (MemoryMarshal.Read<uint>(this) >> 24);

	public override int GetHashCode() {
		var hashCode = new HashCode();
		hashCode.AddBytes(MemoryMarshal.AsBytes(WordCount < 5 ? ((ReadOnlySpan<byte>) this)[..(WordCount * 4)] : this));
		return hashCode.ToHashCode();
	}

	public static bool operator ==(PDSKey left, PDSKey right) => left.Equals(right);
	public static bool operator !=(PDSKey left, PDSKey right) => !(left == right);

	public override string ToString() {
		var guid = Convert.ToHexStringLower(WordCount < 5 ? ((ReadOnlySpan<byte>) this)[..(WordCount * 4)] : this);

		return PDSKeyRegistry.Lookup.TryGetValue(this, out var name) ? $"{name} ({guid})" : guid;
	}

	[Conditional("DEBUG")]
	public void CheckKnown() => Debug.Assert(PDSKeyRegistry.Lookup.ContainsKey(this));
}
