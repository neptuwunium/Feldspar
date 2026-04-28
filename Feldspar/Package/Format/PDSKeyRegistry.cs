// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

namespace Feldspar.Package.Format;

public static class PDSKeyRegistry {
	static PDSKeyRegistry() {
		Lookup[NameMap = new PDSKey("1010b004628242a02092898a000a09a0")] = nameof(NameMap);
		Lookup[EntryMap = new PDSKey("c020b0054a1506861c44144c3040c28600000014")] = nameof(EntryMap);
		Lookup[Count = new PDSKey("08215005a782142a8e5891d05419d0a200000001")] = nameof(Count);
		Lookup[NamePartCount = new PDSKey("2160100450654221651b20a80a8a0823")] = nameof(NamePartCount);
		Lookup[TextureResourceId = new PDSKey("1050d003408942a000001e18")] = nameof(TextureResourceId);
		Lookup[MaterialResourceId = new PDSKey("143c4803081130c800000687")] = nameof(MaterialResourceId);
		Lookup[ModelResourceId = new PDSKey("a1947803810f0e4000000030")] = nameof(ModelResourceId);
	}

	public static PDSKey EntryMap { get; }
	public static PDSKey Count { get; }
	public static PDSKey NameMap { get; }
	public static PDSKey NamePartCount { get; }
	public static PDSKey TextureResourceId { get; }
	public static PDSKey MaterialResourceId { get; }
	public static PDSKey ModelResourceId { get; }

	public static Dictionary<PDSKey, string> Lookup { get; } = [];
}
