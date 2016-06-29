using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using PortraitBuilder.UI;

namespace PortraitBuilder {
	static class Program {

		private static readonly ILog logger = LogManager.GetLogger(typeof(Program).Name);

		private static string CK2_EXE = "CK2game.exe";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			logger.Info("Starting application");

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// Add handler for UI thread exceptions and force all WinForms errors to go through handler
			Application.ThreadException += new ThreadExceptionEventHandler(onUIThreadException);
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			// This handler is for catching non-UI thread exceptions
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onUnhandledException);

			Application.ApplicationExit += new EventHandler(onExitApplication);

			if (!Directory.Exists("dlc/"))
				Directory.CreateDirectory("dlc/");

			if (!File.Exists("gamedir")) {
				string dir = null;

				while (dir == null) {
					string ck2exePath = Snippets.OpenFileDialog("Please select the location of your " + CK2_EXE, "Application (*.exe)|*.exe", null);

					if (ck2exePath == null) {
						if (MessageBox.Show("This program cannot run without data from the Crusader Kings II installation directory. \n To find the directory in Steam: right-click the game in the library, Properties / Local Files / Browse Local Files.",
																 "Exit Application?",
																 MessageBoxButtons.YesNo,
																 MessageBoxIcon.Exclamation) == DialogResult.Yes) {
							break;
						}
					}
					else {
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
			}
			else {
				StartUI(args);
			}
		}

		private static void SetDir(string dir) {
			Stream stream = new FileStream("gamedir", FileMode.Create);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(dir);
			stream.Close();
		}

		private static void StartUI(string[] args) {
			if (args.Length > 0 && args[0] == "-logfull") {
				((Hierarchy)LogManager.GetRepository()).Root.Level = Level.Debug;
			}
			try {
				Application.Run(new PortraitBuilderForm());
			}
			catch (Exception e) {
				logger.Fatal("Fatal error." + e);
			}
		}

		private static void onUnhandledException(Object sender, UnhandledExceptionEventArgs e) {
			try {
				Exception exception = (Exception)e.ExceptionObject;
				logger.Fatal("Unhandled exception", exception);
				MessageBox.Show("Unhadled domain exception:\n\n" + exception.Message);
			} finally {
				Application.Exit();
			}
		}

		private static void onUIThreadException(object sender, ThreadExceptionEventArgs t) {
			try {
				logger.Fatal("Unhandled UI thread exception", t.Exception);
				MessageBox.Show("Unhandled UI exception:\n\n" + t.Exception.Message);
			} finally {
				Application.Exit();
			}
		}

		private static void onExitApplication(object sender, EventArgs e) {
			logger.Info("Closing application");
		}
	}
}
