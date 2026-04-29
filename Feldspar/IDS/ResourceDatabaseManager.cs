using Feldspar.IDS.Format;
using Feldspar.IDS.Format.RDB;
using Pluto.IO.FileSystem;

namespace Feldspar.IDS;

public sealed class ResourceDatabaseManager : IDisposable {
	public ResourceDatabaseManager() {
		RDXInfo.ResolveMount = ResolveMount;
		RDXInfo.ResolveLanguage = ResolveLanguage;
	}

	public Dictionary<KTID, ResourceDatabase> Databases { get; } = [];

	public Dictionary<byte, string> MountPaths { get; set; } = new(0xff);
	public Dictionary<byte, string> LanguagePaths { get; set; } = new(0xff);

	public void Dispose() {
		foreach (var value in Databases.Values) {
			value.Dispose();
		}

		Databases.Clear();

		RDXInfo.ResolveMount = RDXInfo.DefaultResolveMount;
		RDXInfo.ResolveLanguage = RDXInfo.DefaultResolveLanguage;
	}

	public void Mount(string path) {
		foreach (var rdbFile in new FileEnumerator(path, new EnumerationOptions { RecurseSubdirectories = true }, "*.rdb")) {
			var db = new ResourceDatabase(rdbFile, this);

			if (Databases.TryGetValue(db.Name, out var existing)) {
				existing.Dispose();
			}

			Databases[db.Name] = db;
		}
	}

	public string ResolveMount(byte value) => MountPaths.GetValueOrDefault(value) ?? RDXInfo.DefaultResolveMount(value);
	public string ResolveLanguage(byte value) => LanguagePaths.GetValueOrDefault(value) ?? RDXInfo.DefaultResolveLanguage(value);

	public void SetMountPointForType(byte type, string? mountPoint) {
		if (string.IsNullOrEmpty(mountPoint)) {
			MountPaths.Remove(type);
		} else {
			MountPaths[type] = mountPoint;
		}
	}

	public void SetLanguageForType(byte type, string language) {
		if (string.IsNullOrEmpty(language)) {
			LanguagePaths.Remove(type);
		} else {
			LanguagePaths[type] = language;
		}
	}

	public void Remount() {
		foreach (var database in Databases.Values) {
			database.Remount();
		}
	}
}
