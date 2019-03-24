using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using PortraitBuilder.Model;
using PortraitBuilder.Parser;
using PortraitBuilder.Model.Content;
using PortraitBuilder.Model.Portrait;
using ICSharpCode.SharpZipLib.Zip;

namespace PortraitBuilder.Engine {

	/// <summary>
	/// Loads content based on hierachical override: vanilla -> DLC -> mod -> dependent mod
	/// </summary>
	public class Loader {

		private static readonly ILog logger = LogManager.GetLogger(typeof(Loader));

		private PortraitTypeMerger portraitTypeMerger = new PortraitTypeMerger();

		/// <summary>
		/// User configuration: game path, etc.
		/// </summary>
		private User user;

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
		/// </summary>
		private List<Content> activeContents = new List<Content>();

		/// <summary>
		/// Merged portraitData of all active content.
		/// </summary>
		private PortraitData activePortraitData = new PortraitData();

		/// <summary>
		/// Vanilla data - never reloaded dynamically
		/// </summary>
		private Content vanilla;

		public Loader(User user) {
			this.user = user;
		}

		public PortraitData GetActivePortraitData() {
			return activePortraitData;
		}

		public PortraitType GetPortraitType(string basePortraitType) {
			return GetActivePortraitData().PortraitTypes[basePortraitType];
		}

		public PortraitType GetPortraitType(string basePortraitType, string clothingPortraitType) {
			return portraitTypeMerger.merge(GetActivePortraitData().PortraitTypes[basePortraitType], GetActivePortraitData().PortraitTypes[clothingPortraitType]);
		}

		public List<Content> GetActiveContents() {
			return activeContents;
		}

		public void LoadVanilla() {
			vanilla = new Content();
			vanilla.Name = "vanilla";
			vanilla.AbsolutePath = user.GameDir;

			logger.Info("Loading portraits from vanilla.");
			vanilla.PortraitData = portraitReader.Parse(user.GameDir);

			// Init
			activePortraitData = vanilla.PortraitData;
			activeContents.Add(vanilla);
		}

		public List<DLC> LoadDLCs(Boolean clean) {
			if (clean) {
				// Cleanup temporary DLC Dir
				Directory.Delete(user.DlcDir, true);
			}
			return LoadDLCs();
		}

		public List<DLC> LoadDLCs() {
			string dlcFolder = Path.Combine(user.GameDir,"DLC");
			logger.Info("Loading DLCs from " + dlcFolder);
			List<DLC> dlcs = dlcReader.ParseFolder(dlcFolder);

			UnzipDLCs(dlcs);

			foreach (DLC dlc in dlcs) {
				logger.Info("Loading portraits from DLC: " + dlc.Name);
				dlc.PortraitData = portraitReader.Parse(dlc.AbsolutePath);
			}
			return dlcs;
		}

		/// <summary>
		/// Unzip DLC, only if tmp folder doesn't already exist
		/// </summary>
		/// <param name="dlcs"></param>
		private void UnzipDLCs(List<DLC> dlcs) {
			FastZip fastZip = new FastZip();
			foreach (DLC dlc in dlcs) {
				string dlcCode = dlc.DLCFile.Replace(".dlc", "");
				string newDlcAbsolutePath = Path.Combine(user.DlcDir, dlcCode);
				if (!Directory.Exists(newDlcAbsolutePath)) {
					logger.Info(string.Format("Extracting {0} to {1}", dlc.Name, newDlcAbsolutePath));
					// Filter only portraits files, to gain speed/space
					string fileFilter = @"interface;gfx/characters";
					fastZip.ExtractZip(dlc.AbsolutePath, newDlcAbsolutePath, fileFilter);

					// In any case, create the directory, so that it is ignored for next load.
					Directory.CreateDirectory(newDlcAbsolutePath);
				}
				dlc.AbsolutePath = newDlcAbsolutePath;
			}
		}

		public List<Mod> LoadMods() {
			List<Mod> mods = new List<Mod>();
			if (Directory.Exists(user.ModDir)) {
				logger.Info("Loading mods from " + user.ModDir);
				mods = modReader.ParseFolder(user.ModDir);
				foreach (Mod mod in mods) {
					if (Directory.Exists(mod.AbsolutePath)) {
						logger.Info("Loading portraits from mod: " + mod.Name);
						mod.PortraitData = portraitReader.Parse(mod.AbsolutePath);
						if(!mod.GetHasPortraitData()){
							mod.Enabled = false;
							mod.DisabledReason = "No portrait data found";
						}
					} else if (mod.AbsolutePath.EndsWith(".zip")) {
						mod.Enabled = false;
						mod.DisabledReason = "Archive format is not supported by PortraitBuilder";
						logger.Warn("Mod " + mod.Name + " is using archive format, which is not supported by PortraitBuilder");
					} else {
						mod.Enabled = false;
						mod.DisabledReason = "Mod path does not not exist";
						logger.Error("Mod path " + mod.AbsolutePath + " does not exist");
					}
				}
			} else {
				logger.Error("Mod directory " + user.ModDir +  " doesn't exist");
			}

			return mods;
		}

		public void ActivateContent(Content content) {
			// TODO load order
			activeContents.Add(content);
			RefreshContent(content);
		}

		public void DeactivateContent(Content content) {
			activeContents.Remove(content);
			content.Unload();
		}

		public void UpdateActiveAdditionalContent(List<Content> contents) {
			foreach (Content content in activeContents) {
				if (!contents.Contains(content)) {
					//Unload sprites
					content.Unload();
				}
			}

			activeContents.Clear();
			activeContents.Add(vanilla);
			activeContents.AddRange(contents);
		}

		public void RefreshContent(Content content) {
			logger.Info("Refreshing content: " + content.Name);
			content.Unload();
			content.PortraitData = portraitReader.Parse(content.AbsolutePath);

			LoadPortraits();
		}

		private void MergePortraitData() {
			activePortraitData = new PortraitData();

			activePortraitData.MergeWith(vanilla.PortraitData);
			// Recalculate merged portrait data
			foreach (Content content in activeContents) {
				activePortraitData.MergeWith(content.PortraitData);
			}
		}

		public void LoadPortraits() {
			MergePortraitData();

			// Apply external offsets
			foreach (KeyValuePair<string, PortraitType> pair in activePortraitData.PortraitTypes) {
				PortraitType portraitType = pair.Value;

				foreach (Layer layer in portraitType.Layers) {
					if (activePortraitData.Offsets.ContainsKey(layer.Name)) {
						layer.Offset = activePortraitData.Offsets[layer.Name];
						logger.Debug(string.Format("Overriding offset of layer {0} to {1}", layer.Name, layer.Offset));
					}
				}
			}
		}
	}
}
