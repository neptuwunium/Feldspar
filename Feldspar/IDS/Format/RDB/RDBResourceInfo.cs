// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using Pluto.SourceGen.BitStructGenerator;

namespace Feldspar.IDS.Format.RDB;

[BitStruct(4)]
public partial struct RDBResourceInfo {
	[BitField(16)] public partial ushort Unknown1 { get; set; }
	[BitField(4)] public partial RDBLocationType Location { get; set; }
	[BitField(4)] public partial RDBStorageType Storage { get; set; }
	[BitField(4)] public partial ushort Unknown4 { get; set; }
	[BitField(2)] public partial ushort Unknown5 { get; set; }
	[BitField(2)] public partial ushort Unknown6 { get; set; }

	public bool IsVirtual => Unsafe.BitCast<RDBResourceInfo, uint>(this) == uint.MaxValue;

	public static RDBResourceInfo Virtual { get; } = Unsafe.BitCast<uint, RDBResourceInfo>(uint.MaxValue);
}
