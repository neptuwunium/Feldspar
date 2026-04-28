// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Feldspar.Package.Format.IDSOBJ;

namespace Feldspar.Package.Format.Muscle;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 0x8)]
public record struct MuscleBlockTableRecord {
	public KTID Name { get; set; }
	public int Index { get; set; }
}
