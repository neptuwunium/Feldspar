// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Feldspar.IDS.Format;

namespace Feldspar.IDS;

[AttributeUsage(AttributeTargets.Class)]
public sealed class IDSTypeAttribute : Attribute {
	public IDSTypeAttribute(uint typeId) => TypeInfoId = typeId;
	public IDSTypeAttribute(string name) => TypeInfoId = KTID.CreateKTID(name);

	public KTID TypeInfoId { get; }
}
