using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PortraitBuilder.UI {
	static class Snippets {

		public static string OpenFileDialog(string sTitle, string sFilter, string sReturn) {
			using (OpenFileDialog dFileDialog = new OpenFileDialog()) {
				dFileDialog.Title = sTitle;
				dFileDialog.CheckFileExists = true;
				dFileDialog.CheckPathExists = true;
				dFileDialog.Filter = sFilter;
                dFileDialog.DereferenceLinks = true;


                if (dFileDialog.ShowDialog() == DialogResult.OK) {
					return dFileDialog.FileName;
				} else {
					return sReturn;
				}
			}
		}

		public static string SaveFileDialog(string sTitle, string sFilter, string sReturn) {
			using (SaveFileDialog dFileDialog = new SaveFileDialog()) {
				dFileDialog.Title = sTitle;
				dFileDialog.CheckPathExists = true;
				dFileDialog.Filter = sFilter;

				if (dFileDialog.ShowDialog() == DialogResult.OK) {
					return dFileDialog.FileName;
				} else {
					return sReturn;
				}
			}
		}
	}
}
