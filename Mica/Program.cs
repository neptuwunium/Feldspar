// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Feldspar.IDS;
using Serilog;

namespace Mica;

internal static class Program {
	private static void Main(string[] args) {
		Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateLogger();

		using var rdbdb = new ResourceDatabaseManager();
		rdbdb.Mount(args[0]);
	}
}
