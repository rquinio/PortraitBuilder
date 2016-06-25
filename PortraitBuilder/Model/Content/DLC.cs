using System.Collections.Generic;

namespace PortraitBuilder.Model.Content {
	public class DLC : Content {

		/// <summary>
		/// Name of .dlc file, E.g. dlc060.dlc
		/// </summary>
		public string DLCFile;

		/// <summary>
		/// Relative path to the mod archive, E.g. dlc/dlc060.zip
		/// </summary>
		public string Archive;

		/// <summary>
		/// Checksum of DLC
		/// </summary>
		public string Checksum;

		public int SteamID;
		public int GamersGateID;

		/// <summary>
		/// Wether the DLC affacts the checksum
		/// </summary>
		public bool AffectsChecksum;

		public override string ToString() {
			return string.Format("Name: {0}, Archive: {1}", Name, Archive);
		}
	}
}
