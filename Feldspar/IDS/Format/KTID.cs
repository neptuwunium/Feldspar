// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using Feldspar.Json;
using Pluto.SourceGen.TransparentStructGenerator;

namespace Feldspar.IDS.Format;

[TransparentStruct<uint>, JsonConverter(typeof(KTIDConverter))]
public partial struct KTID {
	public KTID(string text) => this = CreateKTID(text);

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

		if (KTIDRegistry.ReverseLookup.TryGetValue(text, out var hash)) {
			return hash;
		}

		return CreateKTID(text.AsSpan(), text);
	}

	public static KTID CreateKTID(ReadOnlySpan<char> text, string? value = null) {
		var stack = (stackalloc byte[Encoding.UTF8.GetByteCount(text)]);
		var n = Encoding.UTF8.GetBytes(text, stack);
		var id = CreateKTID(stack[1..n], stack[0] * 0x1f);
		KTIDRegistry.Register(value ?? new string(text), id);
		return id;
	}

	public static KTID CreateKTID(ReadOnlySpan<byte> text) => text.Length < 1 ? default : CreateKTID(text[1..], text[0] * 0x1f);

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

	public static bool TryParseStrict(ReadOnlySpan<char> chars, out KTID value) {
		value = default;

		if (chars.Length == 0) {
			return true;
		}

		if (chars.Length != 11) {
			return false;
		}

		if (chars is not ['@', '0', 'x', ..]) {
			return false;
		}

		if (!uint.TryParse(chars[3..11], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hash)) {
			return false;
		}

		value = hash;
		return true;
	}

	public static bool TryParse(ReadOnlySpan<char> chars, out KTID value) {
		value = default;

		if (chars.Length == 0) {
			return true;
		}

		if (chars is not ['@', '0', 'x', ..] || chars.Length < 11) {
			value = CreateKTID(chars);
			return true;
		}

		if (!uint.TryParse(chars[3..11], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hash)) {
			return false;
		}

		value = hash;
		return true;
	}

	public static bool TryParse(string text, out KTID value) => TryParse(text.AsSpan(), out value);

	public static KTID Parse(string text) => !TryParse(text.AsSpan(), out var value) ? throw new FormatException("invalid ktid format") : value;

	public override string ToString() => !IsValid ? "null" : KTIDRegistry.Lookup.TryGetValue(Value, out var text) ? text : $"@0x{Value:x08}";
	public static implicit operator KTID(string value) => TryParse(value, out var ktid) ? ktid : default;

	public bool IsValid => Value != 0;
}
