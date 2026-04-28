// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Feldspar.Package.Format.Decade;

[StructLayout(LayoutKind.Sequential, Pack = 16, Size = 0x10)]
public record struct DecadeTableHeader {
	public int Offset { get; set; }
	public int Count { get; set; }
	public int StringTableSize { get; set; }
}
