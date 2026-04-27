// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Feldspar.Package.Muscle;
using Pluto.IO.FileSystem;

namespace Mica;

internal static class Program {
    private static void Main(string[] args) {
		// using var data = RentedArray<byte>.FromFile(Path.Combine(args[0], "COMMON/dafb4dd62a79856ae4a02584d9f642a10208bcc3d3de61210a73a75bfb218bc0"));
		// var table = new DecadeTable(args[0], data);

		var outputRoot = args[1];

		foreach (var binPath in new FileEnumerator(args[0], new EnumerationOptions { RecurseSubdirectories = true }, "*.bin")) {
			if (binPath.Contains(".rdb", StringComparison.OrdinalIgnoreCase)) {
				continue;
			}

			Console.WriteLine(binPath);

			var name = Path.GetFileNameWithoutExtension(binPath);
			
			using var bin = new MuscleBinFile(binPath);

			foreach (var (ktid, info) in bin.Files) {
				Console.WriteLine($"{ktid} {info}");
				using var fileData = bin.OpenFile(ktid);
				var outputPath = Path.Combine(outputRoot, name, ktid + ".bin");

				if (fileData.Length <= 0) {
					continue;
				}

				var dir = Path.GetDirectoryName(outputPath);
				Directory.CreateDirectory(dir ?? outputRoot);
				File.WriteAllBytes(outputPath, fileData.Span);
			}
		}
		/*
		foreach (var filename in table.Files.Keys) {
			Console.WriteLine(filename);

			try {
				using var fileData = table.OpenFile(filename);

				var outputPath = Path.Combine(outputRoot, filename.SanitizeTraversal());
				if (fileData.Length <= 0) {
					continue;
				}

				var dir = Path.GetDirectoryName(outputPath);
				Directory.CreateDirectory(dir ?? outputRoot);
				File.WriteAllBytes(outputPath, fileData.Span);
			} catch(Exception e) {
				Console.WriteLine($"Failed: {e}");
			}
		}
		*/
		// using var fileData = table.OpenFile("character/p_fot100_1p1c/p_fot100_1p1c.gmpk");
		// using var packageData = new PackageData(fileData, "character/p_fot100_1p1c/p_fot100_1p1c.gmpk");
		// foreach(var (resourceName, index) in packageData.NameLookup) {
		// 	var resource = packageData.Resources[index];
		// 	var outputPath = Path.Combine(outputRoot, $"character/p_fot100_1p1c/{resourceName}");
		// 	Console.WriteLine(outputPath);
		// 	var dir = Path.GetDirectoryName(outputPath);
		// 	Directory.CreateDirectory(dir ?? outputRoot);
		// 	File.WriteAllBytes(outputPath, resource.Span);
		// }
	}
}
