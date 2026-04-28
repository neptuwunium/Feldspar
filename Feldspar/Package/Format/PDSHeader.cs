// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

namespace Feldspar.Package.Format;

public record struct PDSHeader {
	public PackageDataHeader Magic { get; set; }
	public int LookupCount { get; set; }
	public int Unknown1 { get; set; }
	public int Unknown2 { get; set; }
	public int ValueCount { get; set; }
	public int LookupOffset { get; set; }
	public int ValueOffset { get; set; }
}
