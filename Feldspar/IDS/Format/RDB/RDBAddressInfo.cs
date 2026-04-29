// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Feldspar.IDS.Format.RDB;

[Flags]
public enum RDXFlags : byte {
	Unknown1 = 1,
	Unknown2 = 2,
	RDXReference = 4,
	ExternalFile = 8,
	Unknown16 = 0x10,
	Unknown32 = 0x20,
	Unknown64 = 0x40,
	Unknown128 = 0x80,
}

// game defaults BinSubIndex to 0xF. BinIndex cannot be more than 0xFFF
public record struct RDBAddressInfo(long Offset, long Length, int BinIndex = -1, int BinSubIndex = -1, string? ExternalPath = null, RDXInfo Index = new()) {
	private const int OFFSET_IDX = 0;
	private const int LENGTH_IDX = 1;
	private const int BIN_IDX_IDX = 2;
	private const int BIN_SUBIDX_IDX = 3;
	private const int EXT_PATH_IDX = 4;
	private const int MAX_IDX = 5;
	private static ReadOnlySpan<byte> Identifiers => "@#&?\0"u8;

	public RDXFlags IndexFlags { get; set; }
	public byte IndexUnknown { get; set; }
	public bool IsValid => Offset >= 0x10 && Length >= Unsafe.SizeOf<RDBIndexHeader>();

	public string Ext {
		get {
			if (BinSubIndex == -1 && BinIndex == -1) {
				return string.Empty;
			}

			var sb = new StringBuilder();

			if (BinIndex > -1) {
				sb.Append($"{BinIndex}");
			}

			if (BinSubIndex > -1) {
				sb.Append($"_{BinSubIndex}");
			}

			return sb.ToString();
		}
	}

	public string? ExternalPath {
		get;
		set {
			if (Index.IsValid) {
				throw new InvalidOperationException("ExternalPath cannot be set with RDX set");
			}

			field = value;
		}
	} = ExternalPath;

	public RDXInfo Index {
		get;
		set {
			if (!string.IsNullOrEmpty(ExternalPath)) {
				throw new InvalidOperationException("RDX cannot be set with ExternalPath set");
			}

			field = value;
		}
	} = Index;

	public static RDBAddressInfo Parse(ReadOnlySpan<byte> address) => !TryParse(address, out var addressInfo) ? throw new FormatException("address info is not valid") : addressInfo;
	public static RDBAddressInfo Parse(ReadOnlySpan<char> address) => !TryParse(address, out var addressInfo) ? throw new FormatException("address info is not valid") : addressInfo;

	public static bool TryParse(ReadOnlySpan<char> address, out RDBAddressInfo addressInfo) {
		var bytes = (stackalloc byte[Encoding.UTF8.GetByteCount(address)]);
		var n = Encoding.UTF8.GetBytes(address, bytes);
		return TryParse(bytes[..n], out addressInfo);
	}

	public static bool TryParse(ReadOnlySpan<byte> address, out RDBAddressInfo addressInfo) {
		addressInfo = new RDBAddressInfo();

		if (address.Length == 0) {
			return false;
		}

		if (!char.IsAsciiHexDigit((char) address[0])) {
			Debug.Assert(address[0] == 0x1);

			var addressSize = address.Length - 5;
			var addressBytes = addressSize >> 1;
			addressInfo.IndexFlags = (RDXFlags) address[1];
			addressInfo.Offset = ReadVariableInt(2, address);
			addressInfo.Length = ReadVariableInt(addressBytes + 2, address);
			addressInfo.Index = new RDXInfo {
				Index = MemoryMarshal.Read<ushort>(address[(addressSize + 2)..]),
			};
			addressInfo.IndexUnknown = address[addressSize + 4];

			return true;

			long ReadVariableInt(int varOffset, ReadOnlySpan<byte> stack) {
				if (addressBytes is 1 or 2 or 4 or 8) {
					return addressBytes switch {
						1 => stack[varOffset],
						2 => MemoryMarshal.Read<ushort>(stack[varOffset..]),
						4 => MemoryMarshal.Read<uint>(stack[varOffset..]),
						8 => MemoryMarshal.Read<long>(stack[varOffset..]),
						_ => throw new UnreachableException(),
					};
				}

				var result = 0UL;
				for (var i = 0; i < addressBytes; ++i) {
					result |= (ulong) stack[varOffset++] << ((addressBytes - 1 - i) * 8);
				}

				return (long) result;
			}
		}

		var positions = (stackalloc int[MAX_IDX]);
		var lengths = (stackalloc int[MAX_IDX]);
		positions[0] = 0;
		var currentIdx = OFFSET_IDX;

		var id = Identifiers;

		for (var index = 0; index < address.Length; ++index) {
			if (!char.IsAsciiHexDigit((char) address[index])) {
				var nextIdx = id.IndexOf(address[index]);
				if (nextIdx == -1 || nextIdx <= currentIdx) {
					return false;
				}

				currentIdx = nextIdx;
				positions[currentIdx] = index + 1;

				if (currentIdx == MAX_IDX - 1) {
					lengths[currentIdx] = address.Length - index - 1;
					break;
				}

				continue;
			}

			lengths[currentIdx]++;
		}

		if (lengths[OFFSET_IDX] == 0 || lengths[LENGTH_IDX] == 0) {
			return false;
		}

		if (!long.TryParse(address.Slice(positions[OFFSET_IDX], lengths[OFFSET_IDX]), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var offset)) {
			return false;
		}

		addressInfo.Offset = offset;

		if (!long.TryParse(address.Slice(positions[LENGTH_IDX], lengths[LENGTH_IDX]), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var length)) {
			return false;
		}

		addressInfo.Length = length;

		if (lengths[BIN_IDX_IDX] > 0) {
			if (!int.TryParse(address.Slice(positions[BIN_IDX_IDX], lengths[BIN_IDX_IDX]), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var binIdx)) {
				return false;
			}

			addressInfo.BinIndex = binIdx;
		}

		if (lengths[BIN_SUBIDX_IDX] > 0) {
			if (!int.TryParse(address.Slice(positions[BIN_SUBIDX_IDX], lengths[BIN_SUBIDX_IDX]), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var binSubIdx)) {
				return false;
			}

			addressInfo.BinSubIndex = binSubIdx;
		}

		if (positions[EXT_PATH_IDX] > 0) {
			addressInfo.ExternalPath = Encoding.UTF8.GetString(address.Slice(positions[EXT_PATH_IDX], lengths[EXT_PATH_IDX]));
		}

		return true;
	}

	public override string ToString() {
		var sb = new StringBuilder();
		sb.Append($"{Offset:x}@{Length:x}");

		if (BinIndex > -1) {
			sb.Append($"#{BinIndex:x}");
		}

		if (BinSubIndex > -1) {
			sb.Append($"&{BinSubIndex:x}");
		}

		if (!string.IsNullOrEmpty(ExternalPath)) {
			sb.Append($"?{ExternalPath}");
		} else if (Index.IsValid) {
			sb.Append($"?{Index}");
		}

		return sb.ToString();
	}
}
