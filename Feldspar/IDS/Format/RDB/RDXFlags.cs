// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Feldspar.IDS.Format.RDB;

[Flags]
public enum RDXFlags : byte {
	Unknown1 = 1,
	Unknown2 = 2,
	RDXReference = 4,
	ExternalFile = 8,
	Unknown16 = 0x10,
	Unknown32 = 0x20,
	Unknown64 = 0x40,
	Unknown128 = 0x80,
}
