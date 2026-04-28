// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Pluto.SourceGen.MagicGenerator;

namespace Feldspar.Package.Format;

[GenerateMagic]
[Magic("1PDS", "PackageDataSearch", false)]
[Magic("1BPS", "BundlePackageSearch", false)]
[Magic("1DIN", "DataIndexName", false)]
public readonly partial record struct PackageMagic;
