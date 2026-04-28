// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Feldspar.Package.Format;

public record struct DINHeader {
	public PackageDataHeader Magic { get; set; }
	public int LookupCount { get; set; }
	public int NamePartCount { get; set; }
}
