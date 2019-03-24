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

		private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

        private static bool isExiting = false;

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

			if (!Directory.Exists("dlc"))
				Directory.CreateDirectory("dlc");

			if (!File.Exists("gamedir.txt")) {
				string dir = null;

				while (dir == null) {
					string ck2exePath = Snippets.OpenFileDialog("Please select the location of your CK2 game binary", "Executable (*.exe)|*.exe|Application (*.app)|*.app|All files|*", null);

					if (ck2exePath == null || ck2exePath.Length == 0) {
						if (MessageBox.Show("This program cannot run without data from the Crusader Kings II installation directory. \n To find the directory in Steam: right-click the game in the library, Properties / Local Files / Browse Local Files.",
																 "Exit Application?",
																 MessageBoxButtons.YesNo,
																 MessageBoxIcon.Exclamation) == DialogResult.Yes) {
							break;
						}
					}
					else {
                        dir = Path.GetDirectoryName(ck2exePath);
                        if (dir.Length == 0 || !Directory.Exists(Path.Combine(dir, "interface")) || !Directory.Exists(Path.Combine(dir, "gfx"))) {
                            MessageBox.Show("Are you sure you've selected Crusader Kings II game binary (CK2game.exe, ck2 or ck2.app) ? The selected folder doesn't contain expected interface/gfx data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            dir = null;
                            continue;
                        }

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
			Stream stream = new FileStream("gamedir.txt", FileMode.Create);
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(dir);
            writer.Flush();
            stream.Close();
		}

		private static void StartUI(string[] args) {
			if (args.Length > 0 && args[0] == "-logfull") {
				((Hierarchy)LogManager.GetRepository(nameof(PortraitBuilder.Program))).Root.Level = Level.Debug;
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
                // On Mono when exiting: cannot call invoke or BeginInvoke on a control until the window handle is created
                if (!isExiting) {
                    MessageBox.Show("Unhandled exception:\n\n" + exception.Message);
                }
			} finally {
				Application.Exit();
			}
		}

		private static void onUIThreadException(object sender, ThreadExceptionEventArgs t) {
			try {
				logger.Fatal("Unhandled UI thread exception", t.Exception);
                if (!isExiting) {
                    MessageBox.Show("Unhandled UI exception:\n\n" + t.Exception.Message);
                }
			} finally {
				Application.Exit();
			}
		}

		private static void onExitApplication(object sender, EventArgs e) {
			logger.Info("Closing application");
            isExiting = true;
		}
	}
}
