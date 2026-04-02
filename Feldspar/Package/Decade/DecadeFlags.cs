// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

namespace Feldspar.Package.Decade;

[Flags]
public enum DecadeFlags : uint {
	None = 0,
	Streamed = 0x20000000,
	Compressed = 0x40000000,
	Encrypted = 0x80000000,
}
