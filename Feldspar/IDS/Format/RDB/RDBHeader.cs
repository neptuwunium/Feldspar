// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Feldspar.KTGL;

namespace Feldspar.IDS.Format.RDB;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct RDBHeader {
	public ResourceHeader Header { get; set; }
	public int Size { get; set; }
	public KTGLPlatform Platform { get; set; }
	public int Count { get; set; }
	public KTID NameDatabaseId { get; set; }
}
