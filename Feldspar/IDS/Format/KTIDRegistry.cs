// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text;

namespace Feldspar.IDS.Format;

public static class KTIDRegistry {
	public static Dictionary<uint, string> Lookup { get; } = [];
	public static Dictionary<string, uint> ReverseLookup { get; } = [];

	public static void Register(string text, KTID id) {
		Lookup[id] = text;
		ReverseLookup[text] = id;
	}

	public static void Register(ReadOnlySpan<byte> bytes, KTID id) {
		var text = Encoding.UTF8.GetString(bytes);
		Lookup[id] = text;
		ReverseLookup[text] = id;
	}
}
