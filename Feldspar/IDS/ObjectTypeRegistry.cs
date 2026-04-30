// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Reflection;

namespace Feldspar.IDS;

public static class ObjectTypeRegistry {
	static ObjectTypeRegistry() {
		var asm = Assembly.GetExecutingAssembly();

		foreach (var type in asm.GetTypes()) {
			if (!type.IsAssignableTo(typeof(ResourceObject))) {
				continue;
			}

			if (type.GetCustomAttribute<IDSTypeAttribute>() is not { } idsType || idsType.Hash == 0) {
				continue;
			}

			Types[idsType.Hash] = type;
		}
	}

	public static Dictionary<uint, Type> Types { get; } = [];
}
