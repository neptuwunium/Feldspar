using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Feldspar.IDS;
using Feldspar.IDS.Format.OBJ;
using Silk.NET.Maths;

namespace Feldspar.Json;

public class ResourceObjectConverter : JsonConverter<ResourceObject> {
	public override ResourceObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

	public override void Write(Utf8JsonWriter writer, ResourceObject value, JsonSerializerOptions options) {
		writer.WriteStartObject();

		foreach (var (name, (prop, _, _)) in value.Properties) {
			writer.WritePropertyName(name.ToString());

			if (prop.Count == 0 || prop.Type == OBJPropertyType.None) {
				writer.WriteNullValue();
				continue;
			}

			var values = prop.Type switch {
				OBJPropertyType.Bool => value.ReadProperties<bool>(name).Cast<object>(),
				OBJPropertyType.Byte => value.ReadProperties<byte>(name).Cast<object>(),
				OBJPropertyType.Int16 => value.ReadProperties<short>(name).Cast<object>(),
				OBJPropertyType.UInt16 => value.ReadProperties<ushort>(name).Cast<object>(),
				OBJPropertyType.Int32 => value.ReadProperties<int>(name).Cast<object>(),
				OBJPropertyType.UInt32 => value.ReadProperties<uint>(name).Cast<object>(),
				OBJPropertyType.Int64 => value.ReadProperties<long>(name).Cast<object>(),
				OBJPropertyType.UInt64 => value.ReadProperties<ulong>(name).Cast<object>(),
				OBJPropertyType.Float32 => value.ReadProperties<float>(name).Cast<object>(),
				OBJPropertyType.Float64 => value.ReadProperties<double>(name).Cast<object>(),
				OBJPropertyType.Vector4F => value.ReadProperties<Vector4D<float>>(name).Cast<object>(),
				OBJPropertyType.Quaternion => value.ReadProperties<Quaternion<float>>(name).Cast<object>(),
				OBJPropertyType.Vector2F => value.ReadProperties<Vector4D<float>>(name).Cast<object>(),
				OBJPropertyType.Vector3F => value.ReadProperties<Vector3D<float>>(name).Cast<object>(),
				OBJPropertyType.None => throw new UnreachableException(),
				_ => throw new UnreachableException(),
			};

			if (prop.Count > 1) {
				writer.WriteStartArray();
			}

			var wroteAtAll = false;
			foreach (var v in values) {
				wroteAtAll = true;
				JsonSerializer.Serialize(writer, v, options);
			}

			if (prop.Count > 1) {
				writer.WriteEndArray();
			} else if (!wroteAtAll) {
				writer.WriteNullValue();
			}
		}

		writer.WriteEndObject();
	}
}
