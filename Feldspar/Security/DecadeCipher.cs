// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Feldspar.Security;

public static class DecadeCipher {
	public static void Crypt(Span<byte> data, int seed, DecadeKeyRing keyRing) {
		var mag = unchecked((uint) ((uint) seed * keyRing.KeyMultiplier / keyRing.KeyDivisor));
		var magBytes = MemoryMarshal.AsBytes(new Span<uint>(ref mag));
		var keySize = 4 - magBytes.Count((byte) 0);
		Span<byte> key = stackalloc byte[keyRing.KeyMaterial.Length * (keySize == 4 ? 1 : keySize)];
		Span<byte> xorKey = stackalloc byte[keySize];

		var xorKeyIndex = keySize;
		foreach (var @byte in magBytes) {
			if (@byte == 0) continue;

			xorKey[--xorKeyIndex] = @byte;
		}

		for (var index = 0; index < key.Length; index++) {
			key[index] = unchecked((byte) (keyRing.KeyMaterial[index % keyRing.KeyMaterial.Length] ^ xorKey[index % xorKey.Length]));
		}

		for (var i = 0; i < data.Length; ++i) {
			ref var a = ref data[i];
			var b = key[i % key.Length];

			if (a != 0 && a != b) a ^= b;
		}
	}
}

public record DecadeKeyRing {
	public List<string> Manifests { get; set; } = [];
	public ulong KeyMultiplier { get; set; }
	public ulong KeyDivisor { get; set; }
	public byte[] KeyMaterial { get; set; } = [];

	public static DecadeKeyRing VenusVacation { get; } = new() {
		Manifests = ["COMMON/4be7efcc93f64e14d7bd7984ef6eeb2ef5ad802ea74cf12ad50a4d39bab71798", "HIGH/9b2d39b6d57f8613d9a316d8c3e6120886640a5762ea23c8fcc88d8acc7563b4", "LOW/4e172f40859f78ae3b61db95d114f046e39211a356989a418f0069f66a24bb0d"],
		KeyMultiplier = 0x77UL,
		KeyDivisor = 0xBUL,
		KeyMaterial = "WUeq032FJnLBUBmWRnbnL9THAvwpBPY1Bz95ANo/Dlj8Awd2GL3KGXvHfbrGPMr/MkXNgG/sXEuS9PHBy1i/BYtN4qsSHiVYs0VudOaBl28q8b8mXVN9fsQed0/1SQ8SPWlZDSBUTtUieK/oydSfxQ2BGov5O8EBcdT3dGrcmzzqeza48frssgDZS2MyTsDRyuxtfr+CGnFDHyOaYANYrt13vv0WbxoyJOIOQU7zdEmC24MAQoEl7AZaJ7z3YpFBnWb9yFdkQBOrsn4AeDt/C8lsklaoSbdZyE6jEzDpoNlqYm2PmOOL+v0U3IXZ8dyf76uwpz0E3TCP9D7zu9GYaQ=="u8.ToArray(),
	};

	public static DecadeKeyRing Decade { get; } = new() {
		Manifests = ["COMMON/dafb4dd62a79856ae4a02584d9f642a10208bcc3d3de61210a73a75bfb218bc0"],
		KeyMultiplier = 0x69UL,
		KeyDivisor = 0xBUL,
		KeyMaterial = "ahcTaNwLcATREpxtZEXM8n2uuOn44R3QjvCqXKYfrhaee6svbPvfhxjqCBt2ZQ8nX6TxSt4l9IMpOj1lAeATjpDtcGPQDnqn2wZRJ6qkwWoVy2kk8IxM7861NQ6zeMA7zOrdxunA07BshCT2rB7WyTU6D7Qu3fzciFahnemYvHcloUZDe8oEzwIiPzrPrqWJCDVDnv13QCI0ieqrDuUU65C5o7RcBXoL5yugOsAYBlgpLohOw0nuc0mlt86c2TqQ6F44mGeLzUjCsihAANjkX71U6zmiaSjTuGM4FXiKg1iOrdh9wbq6Yc4T1HjV1jV2grl82axAqEUNgXQLOcQvJQzqLzIdUgFH7oy01EC3bZ63R3Q1JqrywTQUmIzV3m3B"u8.ToArray(),
	};
}
