using System.Collections.Generic;

namespace Parsers.Mod {
	public class Mod {
		public string Name;
		public string ModFile;
		public string Path;
		public string UserDir;
		public ModReader.Folder ModPathType;
		public List<string> Dependencies = new List<string>();
		public List<string> Extends = new List<string>();
		public List<string> Replaces = new List<string>();

		public bool HasPortraits = false;

		public override string ToString() {
			return Name;
		}
	}
}