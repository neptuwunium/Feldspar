// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Feldspar.Package.Format.Muscle;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 0x10)]
public record struct MuscleBinHeader {
	public int Version { get; set; }
	public int TypeIdOffset { get; set; }
	public int Count { get; set; }
}
