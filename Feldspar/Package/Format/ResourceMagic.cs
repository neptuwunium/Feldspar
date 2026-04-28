// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Pluto.IO.Binary;
using Pluto.SourceGen.MagicGenerator;

namespace Feldspar.Package.Format;

[GenerateMagic]
[Magic("G1TG", "Texture")]
[Magic("G1M_", "Model")]
[Magic("G1A_", "Animation")]
[Magic("G2A_", "Animation2")]
[Magic("SWGQ", "MaterialInstance", false)]
[Magic("G1S_", "Shader")]
[Magic("G2S_", "Shader2")]
public readonly partial record struct ResourceMagic {
	public string Ext =>
		Value switch {
			Texture => ".g1t",
			Model => ".g1m",
			Animation => ".g1a",
			Animation2 => ".g2a",
			MaterialInstance => ".swg",
			Shader	=> ".g1s",
			Shader2	=> ".g2s",
			_ => string.Empty,
		};

	public static ResourceMagic FromResource(IRentedArray<byte> resource) => MemoryMarshal.Read<ResourceMagic>(resource.Span);
	public static string ToExt(IRentedArray<byte> resource) => FromResource(resource).Ext;
}
