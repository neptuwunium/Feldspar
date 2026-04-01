// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Feldspar.FileTable.Decade;

[StructLayout(LayoutKind.Sequential, Pack = 16, Size = 0x20)]
public record struct DecadeFileInfo {
	public int BlockNameOffset { get; set; }
	public int PathNameOffset { get; set; }
	public DecadeFlags Flags { get; set; }
	public uint Checksum { get; set; }
	public int CompressedSize { get; set; }
	public int Size { get; set; }
}
