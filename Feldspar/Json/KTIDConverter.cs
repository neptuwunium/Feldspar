// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Feldspar.IDS.Format;

namespace Feldspar.Json;

public class KTIDConverter : JsonConverter<KTID> {
	public override KTID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		if (reader.TokenType == JsonTokenType.Number) {
			return reader.GetUInt32();
		}

		if (reader.TokenType == JsonTokenType.String && reader.GetString() is { } str) {
			return str;
		}

		return default;
	}

	public override void Write(Utf8JsonWriter writer, KTID value, JsonSerializerOptions options) {
		if (value == default) {
			writer.WriteNullValue();
		}

		if (KTIDRegistry.Lookup.TryGetValue(value.Value, out var text)) {
			writer.WriteStringValue(text);
		} else {
			writer.WriteNumberValue(value.Value);
		}
	}

	public override KTID ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String || reader.GetString() is not { } str) {
			return default;
		}

		if (str == "@") {
			return default;
		}

		if ((str.StartsWith("@0x") && uint.TryParse(str[3..], NumberStyles.HexNumber, null, out var value)) ||
			(str.StartsWith('@') && uint.TryParse(str[1..], out value))) {
			return value;
		}

		return str;
	}

	public override void WriteAsPropertyName(Utf8JsonWriter writer, KTID value, JsonSerializerOptions options) {
		if (value == default) {
			writer.WritePropertyName("@");
			return;
		}

		writer.WritePropertyName(KTIDRegistry.Lookup.TryGetValue(value.Value, out var text) ? text : value.ToString());
	}
}
