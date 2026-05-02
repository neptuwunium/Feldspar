// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Feldspar.IDS;
using Feldspar.IDS.Format;
using Serilog;

namespace Mica;

internal static class Program {
	private static void Main(string[] args) {
		Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateLogger();

		var ktid = new KTID(0x1ab40ae8);
		var t = ktid.ToString();
		// var a = KTID.CreateKTID("TypeInfo::Object::MotorCharacterSetting");
		// var b = KTID.CreateKTID("TypeInfo::Object::Script::HostFunction::kids::math::IsGreaterArray");
		// var c = KTID.CreateKTID("TypeInfo::Object::Animation::Util::BlendPlayer");
		var d = KTID.CreateKTID("KTGLEffectMeshDataResourceHash");

		using var rdbdb = new ResourceDatabaseManager();
		rdbdb.Mount(args[0]);
	}
}
