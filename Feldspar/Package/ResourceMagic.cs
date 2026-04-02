// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Pluto.IO.Binary;
using Pluto.SourceGen.MagicGenerator;

namespace Feldspar.Package;

[GenerateMagic]
[Magic("G1TG", "Texture")]
[Magic("G1M_", "Model")]
[Magic("G2A_", "Animation2")]
[Magic("SWGQ", "MaterialInstance", false)]
public readonly partial record struct ResourceMagic {
	public string ToExt(IRentedArray<byte> resource) =>
		Value switch {
			Texture => ".g1t",
			Model => ".g1m",
			Animation2 => ".g2a",
			MaterialInstance => ".swg",
			_ => string.Empty,
		};
}
