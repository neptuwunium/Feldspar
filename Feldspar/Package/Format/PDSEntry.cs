// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Feldspar.Package.Format;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 0x10)]
public record struct PDSEntry {
	public uint Flags { get; set; }
	public int EntryIndex { get; set; }
	public int LookupIndex { get; set; }
	public uint Next { get; set; }

	public bool IsHead => (Next & 0x80000000) != 0;
}
