// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using Feldspar.IDS.Format;
using Moonstone;
using Pluto.IO.FileSystem;

KTIDRegistry.Freeze = true;
KTIDRegistry.Lookup.Clear();
KTIDRegistry.ReverseLookup.Clear();

Dictionary<KTID, string> foundTypeNames = [];
Dictionary<KTID, string> foundPropNames = [];

foreach (var rttiPath in new FileEnumerator(args[0], "*.jsonl")) {
	using var rtti = new RTTIEnumerator(rttiPath);

	var dict = new Dictionary<KTID, TypeInfo>();
	foreach (var typeInfo in rtti) {
		dict[typeInfo.Hash] = typeInfo;
	}

	foreach (var typeInfo in dict.Values) {
		var debugName = typeInfo.Name;
		if (foundPropNames.TryGetValue(typeInfo.Hash.Value, out var existing)) {
			if (!existing.Equals(typeInfo.Name, StringComparison.Ordinal)) {
				Console.Error.WriteLine($"[E] Collision! {debugName} {typeInfo.Hash.Value:x08} is colliding with {existing}!");
			}
		} else {
			if (GenerateTypeName(typeInfo) is { } name) {
				debugName = name;
				var id = KTID.CreateKTID(name);
				if (id != typeInfo.Hash) {
					Console.Error.WriteLine($"[E] Mismatch! {debugName} expected {typeInfo.Hash.Value:x08}, got {id.Value:x08}");
				}
			}

			foundTypeNames[typeInfo.Hash] = debugName;
		}


		foreach (var prop in typeInfo.Properties) {
			if (foundPropNames.TryGetValue(prop.Hash.Value, out existing)) {
				if (existing.Equals(prop.Name, StringComparison.Ordinal)) {
					continue;
				}

				Console.Error.WriteLine($"[E] Collision! {debugName} property {prop.Name} {prop.Hash.Value:x08} is colliding with {existing}!");
			}

			var id = KTID.CreateKTID(prop.Name);
			if (id != prop.Hash) {
				Console.Error.WriteLine($"[E] Mismatch! {debugName} property {prop.Name} expected {prop.Hash.Value:x08}, got {id.Value:x08}");
			}

			foundPropNames[prop.Hash] = prop.Name;
		}
	}

	continue;

	string? GenerateTypeName(TypeInfo typeInfo) {
		if (typeInfo.FullName != null) {
			return typeInfo.FullName;
		}

		if (typeInfo.ParentTypes.Count == 0) {
			typeInfo.FullName = typeInfo.Name;
		} else if (dict.TryGetValue(typeInfo.ParentTypes[0], out var parentType) && GenerateTypeName(parentType) is { } parent) {
			typeInfo.FullName = $"{parent}::{typeInfo.Name}";
		} else {
			return null;
		}

		return typeInfo.FullName;
	}
}

// SPDX Notice, obfuscated so reuse doesn't cry.
var preamble = Encoding.UTF8.GetString(Convert.FromBase64String("IyBTUERYLUZpbGVDb3B5cmlnaHRUZXh0OiAyMDI2IE5lcHR1d3VuaXVtCiMgU1BEWC1MaWNlbnNlLUlkZW50aWZpZXI6IENDMC0xLjA="));

using (var propFile = new FileStream("Property.ktid", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)) {
	using var propWriter = new StreamWriter(propFile);
	propWriter.NewLine = "\n";
	propWriter.WriteLine(preamble);
	foreach (var (propId, propName) in foundPropNames.Where(x => x.Value.Length > 0).OrderBy(x => x.Value)) {
		propWriter.WriteLine($"{propId}⇒{propName}");
	}
}

using (var typeFile = new FileStream("Type.ktid", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)) {
	using var typeWriter = new StreamWriter(typeFile);
	typeWriter.NewLine = "\n";
	typeWriter.WriteLine(preamble);
	foreach (var (typeId, typeName) in foundTypeNames.Where(x => x.Value.Length > 0).OrderBy(x => x.Value)) {
		typeWriter.WriteLine($"{typeId}⇒{typeName}");
	}
}
