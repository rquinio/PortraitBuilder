using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Portrait_Builder {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if (!Directory.Exists("dlc/"))
				Directory.CreateDirectory("dlc/");

			if (!File.Exists("gamedir")) {
				string dir = null;

				while (dir == null) {
					dir = Measter.Snippets.FolderBrowser("Please select the Crusader Kings II directory.", null);

					if (dir == null) {
						if (MessageBox.Show("This program cannot run without data from the Crusader Kings II directory. Do you wish to exit?",
																 "Exit Application?",
																 MessageBoxButtons.YesNo,
																 MessageBoxIcon.Exclamation) == DialogResult.Yes) {
							break;
						}
					} else {
						if (!File.Exists(dir + "\\CK2game.exe")) {
							MessageBox.Show("Crusader Kings II not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
							dir = null;
							continue;
						}
						SetDir(dir);

						StartUI(args);
					}
				}
			} else {
				StartUI(args);
			}
		}

		private static void SetDir(string dir) {
			using (Stream stream = new FileStream("gamedir", FileMode.Create))
			using (BinaryWriter writer = new BinaryWriter(stream)) {
				writer.Write(dir);
			}
		}

		private static void StartUI(string[] args) {
			if (args.Length > 0 && args[0] == "-log") {
				Application.Run(new Form1(true, false));
			} else if (args.Length > 0 && args[0] == "-logfull") {
				Application.Run(new Form1(true, true));
			} else {
				Application.Run(new Form1());
			}
		}
	}
}
