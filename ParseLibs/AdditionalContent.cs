using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsers {

	public abstract class AdditionalContent {

		/// <summary>
		/// Name to be displayed, E.g. "My Mod"
		/// </summary>
		public string Name;

		/// <summary>
		/// Absolute path to the content root folder
		/// </summary>
		public string AbsolutePath;

		public bool HasPortraits = false;

		public override string ToString() {
			return Name;
		}
	}
}
