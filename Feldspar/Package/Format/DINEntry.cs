// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Feldspar.Package.Format;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 0x8)]
public record struct DINEntry {
	public int Index { get; set; }
	public uint Next { get; set; }

	public bool IsHead => (Next & 0x80000000) != 0;
}
