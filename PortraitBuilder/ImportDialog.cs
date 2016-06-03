using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Portrait_Builder {
	public partial class ImportDialog : Form {
		private bool isDNAValid = false; 
		private bool isPropertiesValid = false;

		public char Neck = 'a';
		public char Chin = 'a';
		public char Mouth = 'a';
		public char Nose = 'a';
		public char Cheeks = 'a';
		public char Eyes = 'a';
		public char Ears = 'a';
		public char HairColour = 'a';
		public char EyeColour = 'a';

		public char Background = 'a';
		public char Hair = 'a';
		public char prop3 = 'a';
		public char Clothes = 'a';
		public char Beard = 'a';
		public char Headgear = 'a';
		public char Prison = 'a';
		public char Scars = 'a';
		public char RedDots = 'a';
		public char Boils = 'a';
		public char Blinded = 'a';
		public char Player = 'a';

		public ImportDialog() {
			InitializeComponent();
		}

		private void btnCancel_Click(object sender, EventArgs e) {
			Close();
		}

		private void btnOK_Click(object sender, EventArgs e) {
			//Get DNA
			string dna = tbDNA.Text;
			Neck = dna[0];
			Chin = dna[1];
			Mouth = dna[2];
			Nose = dna[3];
			Cheeks = dna[4];
			//unused
			Eyes = dna[6];
			Ears = dna[7];
			HairColour = dna[8];
			EyeColour = dna[9];

			//Properties
			string properties = tbProperties.Text;
			Background = properties[0];
			Hair = properties[1];
			//unused
			Clothes = properties[3];
			Beard = properties[4];
			Headgear = properties[5];
			Prison = properties[6];
			Scars = properties[7];
			RedDots = properties[8];
			Boils = properties[9];
			Blinded = properties[10];
			Player = properties[11];

			DialogResult = DialogResult.OK;
			Close();
		}

		private void tb_TextChanged(object sender, EventArgs e) {
			TextBox tb = (TextBox)sender;

			if (tb == tbDNA) {
				isDNAValid = Validate(tb, 11);
			} else if (tb == tbProperties) {
				isPropertiesValid = Validate(tb, 12);
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
