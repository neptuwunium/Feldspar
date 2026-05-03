// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Feldspar.IDS.Format;

public static class KTIDRegistry {
	public const char SEPARATOR = '⇒';

	static KTIDRegistry() {
		ParseTypeFile("Resources/TypeInfo/Type.ktid");
		ParseTypeFile("Resources/TypeInfo/Property.ktid");
	}

	public static Dictionary<uint, string> Lookup { get; } = [];
	public static Dictionary<string, uint> ReverseLookup { get; } = [];
	public static bool Freeze { get; set; }

	private static void ParseTypeFile(string path) {
		var asm = Assembly.GetExecutingAssembly();
		using var resource = asm.GetManifestResourceStream($"{asm.GetName().Name!}.{path.Replace('/', '.')}");
		if (resource != null) {
			ParseTypeFile(resource);
		}

		if (!File.Exists(path)) {
			return;
		}

		using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		ParseTypeFile(file);
	}

	public static void ParseTypeFile(Stream stream) {
		if (Freeze) {
			return;
		}

		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream) {
			var line = reader.ReadLine()?.Trim();
			// min length is @0x00000000⇒.
			if (string.IsNullOrEmpty(line) || line.Length < 13) {
				continue;
			}

			var chars = line.AsSpan();
			if (chars is not ['@', '0', 'x', _, _, _, _, _, _, _, _, SEPARATOR, ..]) {
				continue;
			}

			if (!KTID.TryParseStrict(chars[..11], out var id)) {
				continue;
			}

			var name = chars[12..];

			var text = new string(name);
			if (Lookup.TryGetValue(id, out var existing)) {
				Debug.WriteLineIf(!existing.Equals(text, StringComparison.Ordinal), "KTID Hash Collision");
				continue;
			}

			Lookup[id] = text;
			ReverseLookup[text] = id;
		}
	}

	public static void Register(string text, KTID id) {
		if (Freeze) {
			return;
		}

		if (!Lookup.TryAdd(id, text)) {
			return;
		}

		Lookup[id] = text;
		ReverseLookup[text] = id;
	}

	public static void Register(ReadOnlySpan<byte> bytes, KTID id) {
		if (Freeze) {
			return;
		}

		var text = Encoding.UTF8.GetString(bytes);

		if (!Lookup.TryAdd(id, text)) {
			return;
		}

		ReverseLookup[text] = id;
	}
}
