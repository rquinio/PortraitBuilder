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

		/// <summary>
		/// True if content has portraitTypes.
		/// Checking sprites would cause too many false positives.
		/// </summary>
		/// <returns></returns>
		public bool GetHasPortraitData() {
			return PortraitData != null && PortraitData.PortraitTypes.Count > 0;
		}
	}
}
