// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

namespace Feldspar.Package;

public record struct DINHeader {
	public PackageDataHeader Magic { get; set; }
	public int LookupCount { get; set; }
	public int NamePartCount { get; set; }
}
