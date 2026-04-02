// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Feldspar.Package;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 0x10)]
public record struct PackageDataHeader {
	public ulong Magic { get; set; }
	public PackageMagic DataSystem { get; set; }
	public int Size { get; set; }
}

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 0x30)]
public record struct PackageDataHeader101 {
	public PackageDataHeader Header { get; set; }
	public int TotalSize { get; set; }
	public int OffsetCount { get; set; }
	public int SizeCount { get; set; }
	public int Padding { get; set; }
	public int OffsetListOffset { get; set; }
	public int SizeListOffset { get; set; }
	public int NameMapOffset { get; set; }
}
