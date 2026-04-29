// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Pluto.SourceGen.MagicGenerator;

namespace Feldspar.Package.Format;

[GenerateMagic]
[Magic("1PDS", "PackageDataSearch", little: false)]
[Magic("1BPS", "BundlePackageSearch", little: false)]
[Magic("1DIN", "DataIndexName", little: false)]
public readonly partial record struct PackageMagic;
