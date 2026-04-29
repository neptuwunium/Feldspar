// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using System.Text;

namespace Feldspar.IDS.Format.RDB;

public delegate string RDXResolver(byte value);

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x8)]
public readonly record struct RDXInfo(ushort Index, byte MountId, byte LanguageId, KTID FDataId) {
	public static RDXResolver ResolveMount { get; set; } = DefaultResolveMount;
	public static RDXResolver ResolveLanguage { get; set; } = DefaultResolveLanguage;

	public bool CanRemount => MountId != 0xFF || LanguageId != 0xFF;

	public static string MountPrefix { get; set; } = "MountPoint_";
	public static string Language { get; set; } = "enUS";

	public static string DefaultResolveMount(byte value) => $"{MountPrefix}{value}";
	public static string DefaultResolveLanguage(byte value) => Language;

	public override string ToString() {
		var sb = new StringBuilder();

		if (MountId != 0xFF && ResolveMount(MountId) is var mountPoint && !string.IsNullOrEmpty(mountPoint)) {
			sb.Append(mountPoint);
			sb.Append('/');
		}

		if (LanguageId != 0xFF && ResolveMount(MountId) is var languagePath && !string.IsNullOrEmpty(languagePath)) {
			sb.Append(languagePath);
			sb.Append('/');
		}

		sb.Append($"0x{FDataId.Value:x8}.fdata");

		return sb.ToString();
	}
}
