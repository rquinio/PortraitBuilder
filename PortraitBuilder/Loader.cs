using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parsers.DLC;
using Parsers.Mod;
using Parsers.Portrait;
using Parsers;
using ICSharpCode.SharpZipLib.Zip;
using log4net;

namespace Portrait_Builder {

	/// <summary>
	/// Loads content based on hierachical override: vanilla -> DLC -> mod -> dependent mod
	/// </summary>
	public class Loader {

		private static readonly ILog logger = LogManager.GetLogger(typeof(Loader).Name);

		/// <summary>
		/// User configuration: game path, etc.
		/// FIXME Should be private
		/// </summary>
		public User user;

		/// <summary>
		/// Stateless mod scanner
		/// </summary>
		private ModReader modReader = new ModReader();

		/// <summary>
		/// Stateless dlc scanner
		/// </summary>
		private DLCReader dlcReader = new DLCReader();

		/// <summary>
		/// Stateless portraits.gfx file scanner
		/// </summary>
		private PortraitReader portraitReader = new PortraitReader();

		/// <summary>
		/// DLCs or Mods that are checked
		/// FIXME Should be private
		/// </summary>
		public List<Content> activeContents = new List<Content>();

		/// <summary>
		/// Merged portraitData of all active content.
		/// 
		/// FIXME Should be private
		/// </summary>
		public PortraitData activePortraitData = new PortraitData();

		/// <summary>
		/// Vanilla data - never reloaded dynamically
		/// </summary>
		private Content vanilla;

		public Loader(User user) {
			this.user = user;
		}

		public void LoadVanilla() {
			vanilla = new Content();
			vanilla.Name = "vanilla";
			vanilla.AbsolutePath = user.GameDir;
		}

		public List<DLC> LoadDLCs() {
			logger.Info("Loading DLCs from " + user.GameDir + @"dlc\");
			List<DLC> dlcs = dlcReader.ParseFolder(user.GameDir + @"\dlc");

			FastZip fastZip = new FastZip();
			foreach (DLC dlc in dlcs) {
				string dlcCode = dlc.DLCFile.Replace(".dlc", "");
				string newDlcAbsolutePath = user.DlcDir + dlcCode + @"\";
				logger.Info(string.Format("Extracting {0} to {1}", dlc.Name, newDlcAbsolutePath));

				// Filter only portraits files, to gain speed/space
				string fileFilter = @"interface;gfx/characters";
				fastZip.ExtractZip(dlc.AbsolutePath, newDlcAbsolutePath, fileFilter);
				dlc.AbsolutePath = newDlcAbsolutePath;
			}
			return dlcs;
		}

		public List<Mod> LoadMods() {
			logger.Info("Loading mods from " + user.MyDocsDir + @"\mod\");
			List<Mod> mods = modReader.ParseFolder(user.MyDocsDir + @"\mod\");

			foreach (Mod mod in mods) {

				if (!Directory.Exists(mod.AbsolutePath))
					continue;
			}
			return mods;
		}

		public void UpdateActivateAdditionalContent(List<Content> contents) {
			activeContents.Clear();
			activeContents.Add(vanilla);
			activeContents.AddRange(contents);

			//MergePortraitData();
		}

		private void MergePortraitData() {
			logger.Info("Disposing of previous portrait data.");
			activePortraitData.Dispose();

			activePortraitData.MergeWith(vanilla.PortraitData);
			// Recalculate merged portrait data
			foreach (Content content in activeContents) {
				activePortraitData.MergeWith(content.PortraitData);
			}
		}

		public void LoadPortraits() {
			logger.Info("Loading portraits from vanilla.");
			vanilla.PortraitData = portraitReader.Parse(user.GameDir);

			foreach (Content content in activeContents) {
				logger.Info("Loading portraits from mod: " + content.Name);
				content.PortraitData = portraitReader.Parse(content.AbsolutePath);
			}

			logger.Debug("Setting up type flags");
			foreach (KeyValuePair<string, PortraitType> pair in activePortraitData.PortraitTypes) {
				PortraitType portraitType = pair.Value;
				logger.Debug(" --Setting up flags for " + portraitType.Name);

				foreach (Layer layer in portraitType.Layers) {
					if (activePortraitData.Offsets.ContainsKey(layer.Name)) {
						layer.Offset = activePortraitData.Offsets[layer.Name];
						logger.Debug(string.Format("Overriding offset of layer {0} to {1}", layer.Name, layer.Offset));
					}
					setupFlags(portraitType, layer);
				}
			}

			MergePortraitData();
		}

		// FIXME This should not be needed, as layer are associated via dna/properties letters already
		private void setupFlags(PortraitType portraitType, Layer layer) {
			// Shared
			setupFlag(portraitType, layer, "background");
			setupFlag(portraitType, layer, "boils");
			setupFlag(portraitType, layer, "reddots");
			setupFlag(portraitType, layer, "scars");
			setupFlag(portraitType, layer, "imprisoned");
			setupFlag(portraitType, layer, "blinded");

			// Properties
			setupFlag(portraitType, layer, "clothes");
			setupFlag(portraitType, layer, "headgear");
			setupFlag(portraitType, layer, "beard");
			setupFlag(portraitType, layer, "hair");

			// DNA
			setupFlag(portraitType, layer, "base");
			setupFlag(portraitType, layer, "neck");
			setupFlag(portraitType, layer, "cheeks");
			setupFlag(portraitType, layer, "chin");
			setupFlag(portraitType, layer, "mouth");
			setupFlag(portraitType, layer, "nose");
			setupFlag(portraitType, layer, "eyes");
			setupFlag(portraitType, layer, "ear");
		}

		private void setupFlag(PortraitType portraitType, Layer layer, string layerName) {
			if (layer.Name.Contains(layerName) && !portraitType.CustomFlags.ContainsKey(layerName))
				portraitType.CustomFlags.Add(layerName, layer.Name);
		}

		// Note: only loads from vanilla
		public string LoadBorders() {
			logger.Debug("Setting up borders.");
			string borderSprite = user.GameDir + @"\gfx\interface\charframe_150.dds";

			if (!File.Exists(borderSprite)) {
				logger.Error("Borders file \\gfx\\interface\\charframe_150.dds not found.");
				return null;
			}
			return borderSprite;
		}
	}
}
