// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Pluto.IO.Binary;

namespace Feldspar.IDS.Objects;

[IDSType(0)] // todo
public class NameDatabase : ResourceObject {
	public NameDatabase(Resource resource, BufferBinaryReader reader) : base(resource) { }
}
