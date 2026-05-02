// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using Feldspar.IDS.Format;
using Feldspar.IDS.Format.OBJ;

namespace Moonstone;

public record TypePropertyInfo {
	public int Count { get; set; }

	[JsonConverter(typeof(JsonStringEnumConverter<OBJPropertyType>))]
	public OBJPropertyType Type { get; set; }

	public string Name { get; set; } = "Unknown";
	public KTID Hash { get; set; }
}

public record TypeInfo {
	public string Name { get; set; } = "Unknown";
	public string Description { get; set; } = string.Empty;
	public KTID Hash { get; set; }
	public bool IsNullType { get; set; }
	[JsonPropertyName("mudTypeSig")] public string? MUDSignature { get; set; }
	public List<TypePropertyInfo> Properties { get; set; } = [];
	public List<KTID> ParentTypes { get; set; } = [];

	internal string? FullName { get; set; }
}

public sealed class RTTIEnumerator : IDisposable, IEnumerable<TypeInfo> {
	public RTTIEnumerator(string path) => Reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

	private static JsonSerializerOptions JsonSerializerOptions { get; } = new() {
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};

	public StreamReader Reader { get; set; }

	public void Dispose() => Reader.Dispose();

	public IEnumerator<TypeInfo> GetEnumerator() {
		while (!Reader.EndOfStream) {
			var line = Reader.ReadLine();
			if (string.IsNullOrWhiteSpace(line)) {
				break;
			}

			yield return JsonSerializer.Deserialize<TypeInfo>(line, JsonSerializerOptions)!;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
