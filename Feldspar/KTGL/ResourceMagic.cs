// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Pluto.IO.Binary;
using Pluto.SourceGen.MagicGenerator;

namespace Feldspar.KTGL;

[GenerateMagic(4, "Ext")]
[Magic("G1M_", "Model", "g1m")]
[Magic("G1A_", "Animation", "g1a")]
[Magic("G2A_", "Animation2", "g2a")]
[Magic("G1S_", "Shader", "g1s")]
[Magic("G2S_", "Shader2", "g2s")]
[Magic("G1L_", "StreamSet", "g1l")]
[Magic("G1N_", "Font", "g1n")]
[Magic("G1H_", "HeadMorph", "g1h")]
[Magic("G1TG", "TextureGroup", "g1t")]
[Magic("G1VS", "VideoSource", "g1v")]
[Magic("G1SC", "SceneCreator", "g1v")]
[Magic("G1CO", "Collision", "g1c")]
[Magic("G1EM", "EffectModel", "g1e")]
[Magic("G1FX", "Effect", "g1fx")]
[Magic("SWGQ", "Swing", "swg", false)]
[Magic("ecb", "ExcelBinaryData", "ecb", false)]
[Magic("EARC", "ElixirArchive", "elixir")]
[Magic("KRD_", "ResourceDatabase", "rdb")]
[Magic("KRDI", "ResourceDatabaseIndex")]
[Magic("KRDP", "ResourceDatabasePackage")]
[Magic("KNR_", "ResourceNameDatabase", "ndb")]
[Magic("KNRI", "ResourceNameDatabaseIndex")]
[Magic("KOD_", "ObjectDatabase", "kidsobjdb")]
[Magic("KODI", "ObjectDatabaseIndex")]
[Magic("KODR", "ObjectDatabaseResource")]
[Magic("KTSR", "SoundResource", "ktsl2stbin", false)]
[Magic("KTSC", "SoundContainer", "ktsl2asbin", false)]
[Magic("KTSS", "SoundSample", "ktss", false)]
[Magic("KOVS", "OggVorbisSound", "kvs", false)]
[Magic("KSCL", "ScreenLayout", "kscl")]
[Magic("KSLT", "ScreenLayoutTexture", "kslt")]
public readonly partial record struct ResourceMagic {
	public static ResourceMagic FromResource(IRentedArray<byte> resource) => MemoryMarshal.Read<ResourceMagic>(resource.Span);
	public static string ToExt(IRentedArray<byte> resource) => FromResource(resource).Ext;
}
