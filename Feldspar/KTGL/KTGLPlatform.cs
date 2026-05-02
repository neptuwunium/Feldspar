// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Feldspar.KTGL;

public enum KTGLPlatform : uint {
	D3D9,
	PS3,
	X360,
	RVL,
	NTR,
	CTR,
	NGP,
	Android,
	IOS,
	Cafe,
	D3D11,
	PS4,
	XBO,
	Unknown13,
	D3D12,
	Unknown15,
	NX,
	Unknown17,
	Unknown18,
	PS5,
	XSX,
}

public static class KTGLPlatformExtensions {
	// replace this with sourcegen
	extension(KTGLPlatform platform) {
		public string EngineName => platform switch {
			KTGLPlatform.D3D9 => "dx9",
			KTGLPlatform.X360 => "x2",
			KTGLPlatform.Android => "and",
			KTGLPlatform.Cafe => "caf",
			KTGLPlatform.D3D11 => "dx11",
			KTGLPlatform.XBO => "x3",
			KTGLPlatform.D3D12 => "dx12",
			KTGLPlatform.XSX => "x4",
			_ => platform.ToString().ToLowerInvariant(),
		};
	}
}
