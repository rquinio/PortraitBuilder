using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net;

namespace Parsers.Mod {
	public class ModReader {

		private static readonly ILog logger = LogManager.GetLogger(typeof(Mod).Name);

		/// <summary>
		/// List of loaded Mods
		/// </summary>
		public List<Mod> Mods = new List<Mod>();

		public void Parse(string filename, Folder dir) {
			if (!File.Exists(filename)) {
				logger.Error(string.Format("File not found: {0}", filename));
				return;
			}

			Mod mod;
			string line;
			FileInfo modFile = new FileInfo(filename);
			mod = new Mod();
			mod.ModFile = modFile.Name;
			mod.ModPathType = dir;

			using (StreamReader reader = new StreamReader(filename, Encoding.GetEncoding(1252))) {
				while ((line = reader.ReadLine()) != null) {
					if (line.StartsWith("#"))
						continue;

					if (line.StartsWith("name"))
						mod.Name = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim();
					if (line.StartsWith("path") || line.StartsWith("archive"))
						mod.Path = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim();
					if (line.StartsWith("user_dir") || line.StartsWith("archive"))
						mod.UserDir = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim();

					if (line.StartsWith("extend"))
						mod.Extends.Add(line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim());
					if (line.StartsWith("replace"))
						mod.Replaces.Add(line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim().Replace('/', '\\'));

					if (line.StartsWith("dependencies")) {
						string[] dependencies = line.Split('=')[1].Split('#')[0].Replace("{", "").Replace("}", "").Trim().Split('"');
						foreach (string dependency in dependencies) {
							if (dependency.Trim() != string.Empty) {
								mod.Dependencies.Add(dependency);
							}
						}
					}
				}
			}
			Mods.Add(mod);
		}

		/// <summary>
		/// Loads all files in the given folder.
		/// </summary>
		/// <param name="folder">Path to the folder containing the files to load.</param>
		/// <param name="dirType">Specifies whether path is CKII install or My Docs.</param>
		public void ParseFolder(string folder, Folder dirType) {
			DirectoryInfo dir = new DirectoryInfo(folder);

			if (!dir.Exists) {
				logger.Error(string.Format("Folder not found: {0}", dir.FullName));
				return;
			}

			FileInfo[] mods = dir.GetFiles("*.mod");

			if (mods.Length == 0) {
				logger.Warn(string.Format("No mods found in folder: {0}", dir.FullName));
				return;
			}

			foreach (FileInfo modFile in mods) {
				Parse(modFile.FullName, dirType);
			}
		}

		public enum Folder {
			CKDir,
			MyDocs,
			DLC
		}
	}
}
