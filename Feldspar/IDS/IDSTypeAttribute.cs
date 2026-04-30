// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Feldspar.IDS.Format;

namespace Feldspar.IDS;

[AttributeUsage(AttributeTargets.Class)]
public sealed class IDSTypeAttribute : Attribute {
	public IDSTypeAttribute(uint hash) => Hash = hash;

	public IDSTypeAttribute(string name) => Hash = KTID.CreateHash(name);

	public uint Hash { get; }
}
