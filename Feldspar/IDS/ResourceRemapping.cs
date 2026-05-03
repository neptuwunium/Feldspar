// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Reflection;
using Feldspar.IDS.Format;

namespace Feldspar.IDS;

public static class ResourceRemapping {
	public const char SEPARATOR = '⇒';

	static ResourceRemapping() {
		ParseRemapFile(Resource, "Resources/Remap/Resource.ktid");
		ParseRemapFile(Shader, "Resources/Remap/PB2.ktid");
	}

	public static Dictionary<KTID, KTID> Resource { get; } = [];
	public static Dictionary<KTID, KTID> Shader { get; } = [];

	private static void ParseRemapFile(Dictionary<KTID, KTID> remap, string path) {
		var asm = Assembly.GetExecutingAssembly();
		using var resource = asm.GetManifestResourceStream($"{asm.GetName().Name!}.{path.Replace('/', '.')}");
		if (resource != null) {
			ParseRemapFile(remap, resource);
		}

		if (!File.Exists(path)) {
			return;
		}

		using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		ParseRemapFile(remap, file);
	}

	public static void ParseRemapFile(Dictionary<KTID, KTID> remap, Stream stream) {
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream) {
			var line = reader.ReadLine()?.Trim();
			// min length is @0x00000000⇒@0x00000000
			if (string.IsNullOrEmpty(line) || line.Length < 23) {
				continue;
			}

			var chars = line.AsSpan()[..23];
			if (chars is not ['@', '0', 'x', _, _, _, _, _, _, _, _, SEPARATOR, '@', '0', 'x', _, _, _, _, _, _, _, _]) {
				continue;
			}

			if (!KTID.TryParseStrict(chars[..11], out var from)) {
				continue;
			}

			if (!KTID.TryParseStrict(chars[12..23], out var to)) {
				continue;
			}

			remap[from] = to;
		}
	}
}
