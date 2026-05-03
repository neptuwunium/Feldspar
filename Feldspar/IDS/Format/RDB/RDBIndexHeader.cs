// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Feldspar.KTGL;

namespace Feldspar.IDS.Format.RDB;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct RDBIndexHeader {
	public ResourceHeader Header { get; set; }
	public long Size { get; set; }
	public long DiskSize { get; set; }
	public long MemorySize { get; set; }
	public int ParamDataSize { get; set; }
	public KTID NameId { get; set; }
	public KTID TypeId { get; set; }
	public RDBResourceInfo Info { get; set; }
	public KTID ResourceId { get; set; }
	public int ParamHeaderCount { get; set; }
}
