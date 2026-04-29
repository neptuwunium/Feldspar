// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Feldspar.IDS.Format.RDB;

public enum RDBStorageType : byte {
	None,
	Zlib, // 32-bit block disk size + zlib data
	Lz4, // 32-bit block disk size + lz4 data
	XorZlib, // XOR(Zlib)
	ChecksumZlib, // 16-bit block disk size + 64-bit checksum + block data
}
