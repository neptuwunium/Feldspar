// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Globalization;
using System.Text;

namespace Feldspar.IDS.Format.RDB;

// game defaults BinSubIndex to 0xF. BinIndex cannot be more than 0xFFF
public record struct RDBAddressInfo(long Offset, int Length, int BinIndex = -1, int BinSubIndex = -1, string? ExternalPath = null, RDXInfo Index = default) {
	private const int OFFSET_IDX = 0;
	private const int LENGTH_IDX = 1;
	private const int BIN_IDX_IDX = 2;
	private const int BIN_SUBIDX_IDX = 3;
	private const int EXT_PATH_IDX = 4;
	private const int MAX_IDX = 5;
	private static ReadOnlySpan<byte> Identifiers => "@#&?\0"u8;

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
			if (Index != default) {
				throw new InvalidOperationException("ExternalPath cannot be set with RDX set");
			}

			field = value;
		}
	} = ExternalPath;

	public RDXInfo Index {
		readonly get => field;
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
		var positions = (stackalloc int[MAX_IDX]);
		var lengths = (stackalloc int[MAX_IDX]);
		positions[0] = 0;
		var currentIdx = OFFSET_IDX;

		addressInfo = new RDBAddressInfo();

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

		if (!int.TryParse(address.Slice(positions[LENGTH_IDX], lengths[LENGTH_IDX]), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var length)) {
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
		} else if (Index != default) {
			sb.Append($"?{Index}");
		}

		return sb.ToString();
	}
}
