using System.Collections.Generic;

namespace Parsers.DLC {
	public class DLC {
		public string DLCFile;
		public string Name;
		public string Archive;
		public string Checksum;

		public int SteamID;
		public int GamersGateID;

		public bool AffectsChecksum;

		public override string ToString() {
			return string.Format("Name: {0}, Archive: {1}", Name, Archive);
		}
	}
}
