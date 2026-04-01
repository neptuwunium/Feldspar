// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Feldspar.FileTable.Decade;
using Pluto.Extensions;
using Pluto.IO.Binary;

namespace Mica;

internal static class Program {
    private static void Main(string[] args) {
		using var data = RentedArray<byte>.FromFile(Path.Combine(args[0], "COMMON/dafb4dd62a79856ae4a02584d9f642a10208bcc3d3de61210a73a75bfb218bc0"));
		var table = new DecadeTable(args[0], data);

		var outputRoot = args[1];
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
	}
}
