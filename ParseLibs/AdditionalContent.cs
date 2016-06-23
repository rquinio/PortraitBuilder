using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Parsers.Portrait;

namespace Parsers {

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

		public override string ToString() {
			return Name;
		}

		public bool GetHasPortraitData() {
			return Directory.Exists(AbsolutePath + @"gfx\characters\");
			// return PortraitData.Sprites.Count > 0 || PortraitData.PortraitTypes.Count > 0;
		}
	}
}
