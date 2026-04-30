// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Feldspar.IDS;

public abstract class ResourceObject : IDisposable {
	protected ResourceObject(Resource resource) => Resource = resource;

	public Resource Resource { get; }

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing) { }
}
