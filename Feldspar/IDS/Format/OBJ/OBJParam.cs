// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Feldspar.IDS.Format.OBJ;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0xC)]
public record struct OBJParam(OBJParamType Type, int Count, KTID Name);
