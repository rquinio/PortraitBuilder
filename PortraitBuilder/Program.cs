using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace Portrait_Builder {
	static class Program {

		private static readonly ILog logger = LogManager.GetLogger(typeof(Program).Name);

		private static string CK2_EXE = "CK2game.exe";

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
					string ck2exePath = Measter.Snippets.OpenFileDialog("Please select the location of your " + CK2_EXE, "Application (*.exe)|*.exe", null);

					//FolderBrowser("Please select the Crusader Kings II directory.", null);

					if (ck2exePath == null) {
						if (MessageBox.Show("This program cannot run without data from the Crusader Kings II installation directory. \n To find the directory in Steam: right-click the game in the library, Properties / Local Files / Browse Local Files.",
																 "Exit Application?",
																 MessageBoxButtons.YesNo,
																 MessageBoxIcon.Exclamation) == DialogResult.Yes) {
							break;
						}
					} else {
						if (!ck2exePath.EndsWith(CK2_EXE)) {
							MessageBox.Show("Crusader Kings II not found. Make sure to select the file " + CK2_EXE, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
							dir = null;
							continue;
						}
						dir = ck2exePath.Replace(CK2_EXE, "");
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
			if (args.Length > 0 && args[0] == "-logfull") {
				((Hierarchy)LogManager.GetRepository()).Root.Level = Level.Debug;
			}
			try {
				Application.Run(new Form1());
			} catch (Exception e) {
				logger.Fatal("Fatal error: " + e.ToString());
			}
			
		}
	}
}
