// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Feldspar.IDS;
using Feldspar.IDS.Format.OBJ;
using Silk.NET.Maths;

namespace Feldspar.Json;

public class ResourceObjectDataConverter : JsonConverter<ResourceObjectData> {
	public override ResourceObjectData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

	public override void Write(Utf8JsonWriter writer, ResourceObjectData value, JsonSerializerOptions options) {
		writer.WriteStartObject();

		foreach (var (name, (param, _, _)) in value.Parameters) {
			writer.WritePropertyName(name.ToString());

			if (param.Count == 0 || param.Type == OBJParamType.None) {
				writer.WriteNullValue();
				continue;
			}

			var values = param.Type switch {
				OBJParamType.Bool => value.ReadParams<bool>(name).Cast<object>(),
				OBJParamType.Byte => value.ReadParams<byte>(name).Cast<object>(),
				OBJParamType.Int16 => value.ReadParams<short>(name).Cast<object>(),
				OBJParamType.UInt16 => value.ReadParams<ushort>(name).Cast<object>(),
				OBJParamType.Int32 => value.ReadParams<int>(name).Cast<object>(),
				OBJParamType.UInt32 => value.ReadParams<uint>(name).Cast<object>(),
				OBJParamType.Int64 => value.ReadParams<long>(name).Cast<object>(),
				OBJParamType.UInt64 => value.ReadParams<ulong>(name).Cast<object>(),
				OBJParamType.Float32 => value.ReadParams<float>(name).Cast<object>(),
				OBJParamType.Float64 => value.ReadParams<double>(name).Cast<object>(),
				OBJParamType.Vector4F => value.ReadParams<Vector4D<float>>(name).Cast<object>(),
				OBJParamType.Matrix4F => value.ReadParams<Matrix4X4<float>>(name).Cast<object>(),
				OBJParamType.Vector2F => value.ReadParams<Vector4D<float>>(name).Cast<object>(),
				OBJParamType.Vector3F => value.ReadParams<Vector3D<float>>(name).Cast<object>(),
				OBJParamType.None => throw new UnreachableException(),
				_ => throw new UnreachableException(),
			};

			if (param.Count > 1) {
				writer.WriteStartArray();
			}

			var wroteAtAll = false;
			foreach (var v in values) {
				wroteAtAll = true;
				JsonSerializer.Serialize(writer, v, options);
			}

			if (param.Count > 1) {
				writer.WriteEndArray();
			} else if (!wroteAtAll) {
				writer.WriteNullValue();
			}
		}

		writer.WriteEndObject();
	}
}
