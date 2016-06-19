using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsers {
	public class User {
		/// <summary>
		/// Path of the Crusader Kings II game directory in Steam. E.g. C:\Paradox\Crusader Kings II
		/// </summary>
		public string GameDir;

		/// <summary>
		/// Path to the My Documents directory.
		/// </summary>
		public string MyDocsDir;

		/// <summary>
		/// Path to the temporary DLC directory, containing unzipped assets
		/// </summary>
		public string DlcDir;

		public override string ToString() {
			return string.Format("CK2 directory {0}, Mod directory: {1}, DLC directory:", GameDir, MyDocsDir, DlcDir);
		}
	}
}
