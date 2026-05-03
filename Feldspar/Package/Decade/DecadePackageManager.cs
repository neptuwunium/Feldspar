// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Feldspar.IDS;

namespace Feldspar.Package.Decade;

public sealed class DecadePackageManager : IDisposable {
	public DecadePackageManager() => throw new NotImplementedException();

	public Dictionary<string, PackageData> Packages { get; } = [];
	public Dictionary<string, Resource> Resources { get; } = [];

	public void Dispose() {
		foreach (var resource in Resources.Values) {
			resource.Dispose();
		}

		Resources.Clear();

		foreach (var package in Packages.Values) {
			package.Dispose();
		}

		Packages.Clear();
	}
}
