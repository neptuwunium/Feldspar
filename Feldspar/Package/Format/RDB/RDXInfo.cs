// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using System.Text;
using Feldspar.Package.Format.IDSOBJ;

namespace Feldspar.Package.Format.RDB;

public delegate void RDXResolver(StringBuilder sb, byte value);

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x8)]
public readonly record struct RDXInfo(ushort Index, byte MountId, byte LanguageId, KTID FDataId) {
	public static RDXResolver ResolveMount { get; set; } = DefaultMount;
	public static RDXResolver ResolveLanguage { get; set; } = DefaultLanguage;

	public static void DefaultMount(StringBuilder sb, byte value) => sb.Append($"MountPoint_{value}");
	public static void DefaultLanguage(StringBuilder sb, byte value) => sb.Append("enUS");

	public override string ToString() {
		var sb = new StringBuilder();

		if (MountId != 0xFF) {
			ResolveMount(sb, MountId);
			sb.Append('/');
		}

		if (LanguageId != 0xFF) {
			ResolveLanguage(sb, LanguageId);
			sb.Append('/');
		}

		sb.Append($"0x{FDataId.Value:x8}.fdata");

		return sb.ToString();
	}
}
