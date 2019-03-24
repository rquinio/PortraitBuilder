using PortraitBuilder.Model.Portrait;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PortraitBuilder.UI {
	public partial class ImportDialog : Form {
		private bool isDNAValid = false; 
		private bool isPropertiesValid = false;

		public Portrait portrait = new Portrait();

		public ImportDialog() {
			InitializeComponent();
		}

		private void btnCancel_Click(object sender, EventArgs e) {
			Close();
		}

		private void btnOK_Click(object sender, EventArgs e) {
			string dna = tbDNA.Text;
			string properties = tbProperties.Text;
			portrait.import(dna, properties);

			DialogResult = DialogResult.OK;
			Close();
		}

		private void tb_TextChanged(object sender, EventArgs e) {
			TextBox tb = (TextBox)sender;

			if (tb == tbDNA) {
				isDNAValid = Validate(tb, Characteristic.DNA.Length);
			} else if (tb == tbProperties) {
				isPropertiesValid = Validate(tb, Characteristic.PROPERTIES.Length);
			}

			if (isDNAValid && isPropertiesValid) {
				btnOK.Enabled = true;
			} else {
				btnOK.Enabled = false;
			}
		}

		private bool Validate(TextBox tb, int length) {
			bool isValid = IsValid(tb.Text, length);
			if (isValid) {
				errorProvider.SetError(tb, string.Empty);
			} else {
				errorProvider.SetError(tb, "Invalid text.");
			}
			return isValid;
		}

		private bool IsValid(string dnaOrProperties, int length) {
			bool valid = true;

			foreach (char c in dnaOrProperties) {
				if (!Char.IsLetterOrDigit(c)) {
					valid = false;
					break;
				}
			}

			if (dnaOrProperties.Length != length)
				valid = false;

			return valid;
		}
	}
}
