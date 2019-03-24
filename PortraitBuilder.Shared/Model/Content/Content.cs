using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PortraitBuilder.Model.Portrait;

namespace PortraitBuilder.Model.Content {

	/// <summary>
	/// Represents a loadable unit of content (vanilla, DLC, mod, ...).
	/// </summary>
	public class Content {

		/// <summary>
		/// Name to be displayed, E.g. "My Mod"
		/// </summary>
		public string Name;

		/// <summary>
		/// Absolute path to the content root folder
		/// </summary>
		public string AbsolutePath;

		public PortraitData PortraitData;

		/// <summary>
		/// Watcher on content data changes
		/// </summary>
		public FileSystemWatcher Watcher;

		/// <summary>
		/// Whether the content is supported by this tool and can be enabled
		/// </summary>
		public bool Enabled = true;

		/// <summary>
		/// Reason why the content is not enabled.
		/// </summary>
		public string DisabledReason;

		public override string ToString() {
			return Name;
		}

		/// <summary>
		/// True if either:
		/// - content has portraitTypes .gfx definitions
		/// - or some sprites are overriden (ex: Mediteranean Portraits DLC)
		/// 
		/// Checking any kind of sprites would cause too many false positives (unit DLCs, ...).
		/// </summary>
		/// <returns></returns>
		public bool GetHasPortraitData() {
			return (PortraitData != null && PortraitData.PortraitTypes.Count > 0) || Directory.Exists(AbsolutePath + @"gfx\characters\");
		}

		public void Unload() {
			if (PortraitData != null) {
				PortraitData.Unload();
			}
		}

		public void Dispose() {
			if (PortraitData != null) {
				PortraitData.Dispose();
			}
		}
	}
}
