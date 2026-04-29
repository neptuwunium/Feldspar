// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using Feldspar.KTGL;
using Feldspar.Package.Format;
using Pluto;
using Pluto.IO.Binary;

namespace Feldspar.Package;

public sealed class PackageData : IDisposable {
	public PackageData(IRentedArray<byte> data, string resourceName, bool leaveOpen = false) {
		Buffer = data;
		LeaveOpen = leaveOpen;
		using var reader = new ArrayPoolBinaryReader(data, true);

		using var ds = ReadDataSystem(reader, out var header);
		Header = header;

		Resources.Clear();

		switch (Header.DataSystem.Value) {
			case 0x1010000: {
				reader.Position = 0;
				var legacy = reader.Read<PackageDataHeader101>();
				EntryCount = legacy.OffsetCount;
				Debug.Assert(legacy.OffsetCount == legacy.SizeCount);

				reader.Position = legacy.OffsetListOffset;
				Span<int> offsets = stackalloc int[legacy.OffsetCount];
				reader.Read(offsets);

				reader.Position = legacy.SizeListOffset;
				Span<int> sizes = stackalloc int[legacy.SizeCount];
				reader.Read(sizes);

				for (var index = 0; index < offsets.Length; index++) {
					var offset = offsets[index];
					var length = sizes[index];
					Resources.Add(new UnownedRentedArray<byte>(Buffer, offset, length));
				}

				if (legacy.NameMapOffset > 0) {
					reader.Position = legacy.NameMapOffset;
					NameMap = new PackageDataNameMap(reader);
					var baseName = Path.GetFileNameWithoutExtension(resourceName);

					for (var index = 0; index < Math.Min(NameMap.Names.Count, Resources.Count); index++) {
						var str = NameMap.Names[index];
						NameLookup[baseName + "_" + str + ResourceMagic.ToExt(Resources[index])] = index;
					}

					Debug.Assert(NameLookup.Count == Resources.Count);
				}

				break;
			}
			case PackageMagic.PackageDataSearch:
			case PackageMagic.BundlePackageSearch: {
				PDS = new PackageDataDataSearch(ds);

				if (PDS.Entries.TryGetValue(PDSKeyRegistry.EntryMap, out var entryMap) && entryMap.Offset > 0) {
					ds.Position = entryMap.Offset;
					using var entryReader = ReadDataSystem(ds, out _);
					EntryMap = new PackageDataEntryMap(entryReader);

					if (PDS.Entries.TryGetValue(PDSKeyRegistry.Count, out var count) && count.Offset > 0) {
						ds.Position = count.Offset;
						EntryCount = ds.Read<int>();
					}

					reader.Position = header.Size;
					Span<int> info = stackalloc int[EntryCount * 2];
					reader.Read(info);

					for (var index = 0; index < EntryCount * 2; index += 2) {
						Resources.Add(new UnownedRentedArray<byte>(Buffer, info[index] + header.Size, info[index + 1]));
					}
				}

				if (PDS.Entries.TryGetValue(PDSKeyRegistry.NameMap, out var nameMap) && nameMap.Offset > 0) {
					ds.Position = nameMap.Offset;
					using var nameReader = ReadDataSystem(ds, out _);
					NameMap = new PackageDataNameMap(nameReader);

					if (EntryMap is { } map && NameMap.Names.Count > 0) {
						var str = NameMap.Names[0];
						if (map.TextureResourceId is { } textureResourceId) {
							NameLookup[str + ResourceMagic.ToExt(Resources[textureResourceId])] = textureResourceId;
						}

						if (map.MaterialResourceId is { } materialResourceId) {
							NameLookup[str + ResourceMagic.ToExt(Resources[materialResourceId])] = materialResourceId;
						}

						if (map.ModelResourceId is { } modelResourceId) {
							NameLookup[str + ResourceMagic.ToExt(Resources[modelResourceId])] = modelResourceId;
						}

						for (var index = 0; index < Math.Min(NameMap.Names.Count - 1, map.PartResourceIndices.Count); ++index) {
							str = NameMap.Names[index + 1];
							NameLookup[str + ResourceMagic.ToExt(Resources[map.PartResourceIndices[index]])] = map.PartResourceIndices[index];
						}

						Debug.Assert(NameLookup.Count == Resources.Count);
					} else if (NameLookup.Count == Resources.Count) {
						for (var index = 0; index < Resources.Count; ++index) {
							var str = NameMap.Names[index];
							NameLookup[str + ResourceMagic.ToExt(Resources[index])] = index;
						}
					}
				}

				break;
			}
		}

		if (NameLookup.Count == 0 && Resources.Count == 1) {
			NameLookup[Path.GetFileNameWithoutExtension(resourceName) + ResourceMagic.ToExt(Resources[0])] = 0;
		}
	}

	public PackageDataHeader Header { get; }
	public PackageDataDataSearch? PDS { get; }
	public PackageDataNameMap? NameMap { get; set; }
	public PackageDataEntryMap? EntryMap { get; set; }
	public int EntryCount { get; set; }
	public bool LeaveOpen { get; }
	public IRentedArray<byte> Buffer { get; }
	public List<IRentedArray<byte>> Resources { get; private set; } = ObjectPool<List<IRentedArray<byte>>>.Rent();
	public Dictionary<string, int> NameLookup { get; } = new(StringComparer.OrdinalIgnoreCase);

	public void Dispose() {
		foreach (var entry in Resources) {
			entry.Dispose();
		}

		ObjectPool<List<IRentedArray<byte>>>.Return(Resources);
		Resources = null!;

		if (LeaveOpen) {
			return;
		}

		Buffer.Dispose();
	}

	public static ArrayPoolBinaryReader ReadDataSystem(BufferBinaryReader reader, out PackageDataHeader header) {
		header = reader.Peek<PackageDataHeader>();
		return new ArrayPoolBinaryReader(reader.ReadSharedBytes(header.Size));
	}
}
