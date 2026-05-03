// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Reflection;
using Feldspar.IDS.Format;

namespace Feldspar.IDS;

public static class ObjectTypeRegistry {
	static ObjectTypeRegistry() {
		var asm = Assembly.GetExecutingAssembly();

		foreach (var type in asm.GetTypes()) {
			if (!type.IsAssignableTo(typeof(ResourceObject))) {
				continue;
			}

			if (type.GetCustomAttribute<IDSTypeAttribute>() is not { } idsType || !idsType.TypeInfoId.IsValid) {
				continue;
			}

			Types[idsType.TypeInfoId] = type;
		}
	}

	public static Dictionary<uint, Type> Types { get; } = [];
}
