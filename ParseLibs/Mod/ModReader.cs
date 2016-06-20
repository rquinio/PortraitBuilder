using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net;

namespace Parsers.Mod {
	public class ModReader {

		private static readonly ILog logger = LogManager.GetLogger(typeof(Mod).Name);

		public Mod Parse(string filename) {
			if (!File.Exists(filename)) {
				logger.Error(string.Format("File not found: {0}", filename));
				return null;
			}

			string line;
			FileInfo modFile = new FileInfo(filename);
			Mod mod = new Mod();
			mod.ModFile = modFile.Name;

			StreamReader reader = new StreamReader(filename, Encoding.GetEncoding(1252));
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
			return mod;
		}

		/// <summary>
		/// Loads all files in the given folder.
		/// </summary>
		/// <param name="folder">Path to the folder containing the files to load.</param>
		/// <param name="dirType">Specifies whether path is CKII install or My Docs.</param>
		public List<Mod> ParseFolder(string folder) {
			List<Mod> mods = new List<Mod>();

			DirectoryInfo dir = new DirectoryInfo(folder);

			if (!dir.Exists) {
				logger.Error(string.Format("Folder not found: {0}", dir.FullName));
				return mods;
			}

			FileInfo[] modFiles = dir.GetFiles("*.mod");

			if (modFiles.Length == 0) {
				logger.Warn(string.Format("No mods found in folder: {0}", dir.FullName));
				return mods;
			}

			foreach (FileInfo modFile in modFiles) {
				Mod mod = Parse(modFile.FullName);
				if (mod != null) {
					mod.AbsolutePath = folder + @"\" + mod.Path.Replace("mod/","");
					mods.Add(mod);
				}
			}
			return mods;
		}
	}
}
