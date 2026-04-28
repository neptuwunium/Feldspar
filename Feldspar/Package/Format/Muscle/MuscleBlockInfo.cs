// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Feldspar.Package.Format.Muscle;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 0x28)]
public record struct MuscleBlockInfo {
	public long Offset { get; set; }
	public int MemorySize { get; set; }
	public int CompressedSize { get; set; }
	public int EncryptedSize { get; set; }
	public MuscleBlockHash Hash { get; set; }
	
	public bool IsCompressed => CompressedSize > 0;
	public bool IsEncrypted => EncryptedSize > 0;
}
