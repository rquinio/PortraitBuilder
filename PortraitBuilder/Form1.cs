using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using Parsers.DLC;
using Parsers.Mod;
using Parsers.Portrait;
using Parsers;

namespace Portrait_Builder {

	/// <summary>
	/// Controller class
	/// </summary>
	public partial class Form1 : Form {

		private static readonly ILog logger = LogManager.GetLogger(typeof(Form1).Name);

		private Image previewImage = new Bitmap(176, 176);
		private List<Bitmap> borders = new List<Bitmap>();

		private bool started = false;
		private StringBuilder dnaPropOutput;
		public static Random rand = new Random();

		private Loader loader;

		private PortraitRenderer portraitRenderer = new PortraitRenderer();

		/// <summary>
		/// List of all available DLCs and Mods, indexed by their corresponding checkbox
		/// </summary>
		private Dictionary<CheckBox, Content> usableMods = new Dictionary<CheckBox, Content>();

		/// <summary>
		/// The portrait being previewed
		/// </summary>
		private Portrait portrait = new Portrait();

		/// <summary>
		/// CheckBox for dna, ordered by their dna index.
		/// </summary>
		private List<ComboBox> dnaComboBoxes = new List<ComboBox>();

		/// <summary>
		/// CheckBox for properties, ordered by their properties index.
		/// </summary>
		private List<ComboBox> propertiesComboBoxes = new List<ComboBox>();

		public Form1() {
			InitializeComponent();
			dnaComboBoxes.AddRange(new ComboBox[] { cbNeck, cbChin, cbMouth, cbNose, cbCheeks, null, cbEyes, cbEars, cbHairColour, cbEyeColour });
			propertiesComboBoxes.AddRange(new ComboBox[] { cbBackground, cbHair, null, cbClothes, cbBeard, cbHeadgear, cbPrisoner, cbScars, cbRedDots, cbBoils, cbBlinded, null });

			EnvironmentSetup();
		}

		private void EnvironmentSetup() {
			logger.Info("Portrait Builder Version " + Application.ProductVersion);
			logger.Info("Portrait Builder Parser Library " + Parsers.Version.GetVersion());
			// Add the version to title
			this.Text += " " + Application.ProductVersion;

			User user = new User();
			user.GameDir = ReadGameDir();
			user.MyDocsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Paradox Interactive\Crusader Kings II\";
			user.DlcDir = Environment.CurrentDirectory + @"\dlc\";

			// Cleanup temporary DLC Dir
			Directory.Delete(user.DlcDir, true);

			logger.Info("Configuration: " + user);
			logger.Info("----------------------------");

			loader = new Loader(user);

			loader.LoadVanilla();
			LoadDLCs();
			LoadMods();

			LoadPortraits();
			LoadBorders();

			SetupSharedUI();
			SetupUI();
			RandomizeUI(true);

			DrawPortrait();

			started = true;
		}

		private void LoadBorders() {
			string borderSprite = loader.LoadBorders();

			if(borderSprite != null) {
				Bitmap charFrame = DevIL.DevIL.LoadBitmap(borderSprite);

				for (int i = 0; i < 6; i++) {
					Bitmap border = new Bitmap(176, 176);
					Graphics g = Graphics.FromImage(border);
					g.DrawImage(charFrame, 0, 0, new Rectangle(i * 176, 0, 176, 176), GraphicsUnit.Pixel);
					g.Dispose();
					borders.Add(border);
				}
			}
		}

		private void LoadMods() {
			List<Mod> mods = loader.LoadMods();
			foreach (Mod mod in mods) {
				if (mod.GetHasPortraitData()) {
					registerMod(panelMods, mod);
				}
			}
		}

		private void LoadDLCs() {
			List<DLC> dlcs = loader.LoadDLCs();
			foreach (DLC dlc in dlcs) {
				if (dlc.GetHasPortraitData()) {
					registerMod(panelDLCs, dlc);
				}
			}
		}

		private void registerMod(Control container, Content mod) {
			CheckBox checkbox = new CheckBox();
			checkbox.Text = mod.Name;
			checkbox.AutoEllipsis = true;
			checkbox.Width = 200; // Force overflow
			checkbox.CheckedChanged += new System.EventHandler(this.onCheck);
			checkbox.Padding = new Padding(0);
			checkbox.Margin = new Padding(0);

			container.Controls.Add(checkbox);
			usableMods.Add(checkbox, mod);
		}
		
		private string ReadGameDir() {
			Stream stream = new FileStream("gamedir", FileMode.Open);
			BinaryReader reader = new BinaryReader(stream);
			return reader.ReadString() + @"\";
		}

		private void DrawPortrait() {
			logger.Debug(" --Drawing portrait.");

			Graphics g = Graphics.FromImage(previewImage);

			logger.Debug("   --Clearing preview.");
			g.Clear(Color.Empty);
			Bitmap portraitImage;
			logger.Debug("   --Rendering portrait.");
			try {
				PortraitType portraitType = getSelectedPortraitType();
				portraitImage = portraitRenderer.DrawPortrait(portraitType, portrait, loader.activeContents, loader.user, loader.activePortraitData.Sprites);
			}
			catch (Exception e) {
				logger.Error("Error encountered rendering portrait:" + e.ToString());
				return;
			}
			g.DrawImage(portraitImage, 0, 0);
			logger.Debug("   --Drawing border.");
			g.DrawImage(borders[cbRank.SelectedIndex], 0, 0);

			pbPortrait.Image = previewImage;
		}

		private string GetDNA() {
			logger.Debug(" --Building DNA string.");
			StringBuilder sb = new StringBuilder();

			sb.Append(GetLetter(cbNeck));
			sb.Append(GetLetter(cbChin));
			sb.Append(GetLetter(cbMouth));
			sb.Append(GetLetter(cbNose));
			sb.Append(GetLetter(cbCheeks));
			sb.Append("0"); //Head(Unused)
			sb.Append(GetLetter(cbEyes));
			sb.Append(GetLetter(cbEars));
			sb.Append(GetLetter(cbHairColour));
			sb.Append(GetLetter(cbEyeColour));
			sb.Append("0");

			return sb.ToString();
		}

		private string GetProperties() {
			logger.Debug(" --Building Properties string.");
			StringBuilder sb = new StringBuilder();

			sb.Append(GetLetter(cbBackground));
			sb.Append(GetLetter(cbHair));
			sb.Append("0"); //Base(Unused)
			sb.Append(GetLetter(cbClothes));
			sb.Append(GetLetter(cbBeard));
			sb.Append(GetLetter(cbHeadgear));
			sb.Append(GetLetter(cbPrisoner)); //Imprisoned
			sb.Append(GetLetter(cbScars)); //Scars
			sb.Append(GetLetter(cbRedDots)); //Red Dots
			sb.Append(GetLetter(cbBoils)); //Boils
			sb.Append(GetLetter(cbBlinded));
			sb.Append("b"); // Player overlay

			return sb.ToString();
		}

		// Needs to be called each time portrait object is modified
		private void OutputDNA() {
			logger.Debug(" --Outputting DNA and Property strings.");
			dnaPropOutput = new StringBuilder();

			dnaPropOutput.Append("  dna=\"");
			dnaPropOutput.Append(portrait.GetDNA());
			dnaPropOutput.AppendLine("\"");

			dnaPropOutput.Append("  properties=\"");
			dnaPropOutput.Append(portrait.GetProperties());
			dnaPropOutput.AppendLine("\"");
			tbDNA.Text = dnaPropOutput.ToString();
		}

		private char GetLetter(ComboBox cb) {
			return Portrait.GetLetter(cb.SelectedIndex, cb.Items.Count);
		}

		private void RandomizeUI(bool doRank) {
			logger.Debug("Randomizing UI");
			if (doRank) {
				RandomizeComboBox(cbRank);
			}

			RandomizeComboBox(cbBackground);

			cbScars.SelectedIndex = 0;
			cbRedDots.SelectedIndex = 0;
			cbBoils.SelectedIndex = 0;
			cbPrisoner.SelectedIndex = 0;
			if (cbBlinded.Items.Count > 0) {
				cbBlinded.SelectedIndex = 0;
				cbBlinded.Enabled = true;
			}
			else {
				cbBlinded.Enabled = false;
				cbBlinded.SelectedIndex = -1;
			}
			RandomizeComboBox(cbClothes);
			RandomizeComboBox(cbHeadgear);
			RandomizeComboBox(cbHair);
			RandomizeComboBox(cbBeard);
			RandomizeComboBox(cbNeck);
			RandomizeComboBox(cbCheeks);
			RandomizeComboBox(cbChin);
			RandomizeComboBox(cbMouth);
			RandomizeComboBox(cbNose);
			RandomizeComboBox(cbEyes);
			RandomizeComboBox(cbEars);
			RandomizeComboBox(cbHairColour);
			RandomizeComboBox(cbEyeColour);

			UpdatePortraitDataFromInputs();
		}

		private void RandomizeComboBox(ComboBox cb) {
			if (cb.Items.Count > 0) {
				cb.SelectedIndex = rand.Next(cb.Items.Count - 1);
			}
		}

		private void FillComboBox(ComboBox cb, int count) {
			for (int i = 0; i < count; i++)
				cb.Items.Add(i);
		}

		private void FillComboBox(ComboBox cb, string flagName) {
			cb.Items.Clear();

			PortraitType portraitType = getSelectedPortraitType();
			if (portraitType.CustomFlags.ContainsKey(flagName)) {
				string spriteName = (string)portraitType.CustomFlags[flagName];
				Sprite sprite = loader.activePortraitData.Sprites[spriteName];
				logger.Debug(" --" + flagName + " item count: " + sprite.FrameCount);
				FillComboBox(cb, sprite.FrameCount);
			}
			else {
				logger.Warn(" --No " + flagName + " flag found, setting UI to 26.");
				FillComboBox(cb, 26);
			}
		}

		private PortraitType getSelectedPortraitType() {
			PortraitType selectedPortraitType = null;
			object selectedItem = cbPortraitTypes.SelectedItem;
			if(selectedItem != null) {
				return loader.activePortraitData.PortraitTypes["PORTRAIT_" + selectedItem.ToString()];
			}
			return selectedPortraitType;
		}

		private void SetupSharedUI() {
			logger.Debug("Setting up Shared UI");

			FillComboBox(cbBackground, "background");
			FillComboBox(cbScars, "scars");
			FillComboBox(cbRedDots, "reddots");
			FillComboBox(cbBoils, "boils");
			FillComboBox(cbPrisoner, "imprisoned");
			FillComboBox(cbBlinded, "blinded");
		}

		private void SetupUI() {
			PortraitType portraitType = getSelectedPortraitType();

			logger.Debug("Setting up UI for: " + portraitType.Name);
			FillComboBox(cbClothes, "clothes");
			FillComboBox(cbHeadgear, "headgear");
			FillComboBox(cbHair, "hair");
			FillComboBox(cbBeard, "beard");
			FillComboBox(cbNeck, "neck");
			FillComboBox(cbCheeks, "cheeks");
			FillComboBox(cbChin, "chin");
			FillComboBox(cbMouth, "mouth");
			FillComboBox(cbNose, "nose");
			FillComboBox(cbEyes, "eyes");
			FillComboBox(cbEars, "ear");

			logger.Debug(" --Setting hair colours: " + portraitType.HairColours.Count);
			cbHairColour.Items.Clear();
			for (int i = 0; i < portraitType.HairColours.Count; i++) {
				cbHairColour.Items.Add(i);
			}

			logger.Debug(" --Setting eye colours: " + portraitType.EyeColours.Count);
			cbEyeColour.Items.Clear();
			for (int i = 0; i < portraitType.EyeColours.Count; i++) {
				cbEyeColour.Items.Add(i);
			}
		}

		private void cb_SelectedIndexChanged(object sender, EventArgs e) {
			if (started) {
				UpdatePortraitDataFromInputs();
				DrawPortrait();
			}
		}

		private void btnCopy_Click(object sender, EventArgs e) {
			Clipboard.SetText(dnaPropOutput.ToString());
		}

		private void btnSave_Click(object sender, EventArgs e) {
			string file = Measter.Snippets.SaveFileDialog("Save Image", "PNG|*.png", null);

			if (file != null) {
				previewImage.Save(file, ImageFormat.Png);
			}
		}

		private void btnImport_Click(object sender, EventArgs e) {
			ImportDialog dialog = new ImportDialog();

			if (dialog.ShowDialog(this) == DialogResult.OK) {
				started = false;

				portrait = dialog.portrait;
				OutputDNA();
				UpdateInputsFromPortraitData(portrait);

				started = true;

				DrawPortrait();
			}
		}

		private void btnRandom_Click(object sender, EventArgs e) {
			started = false;
			RandomizeUI(false);
			started = true;

			UpdatePortraitDataFromInputs();
			DrawPortrait();
		}

		/// <summary>
		/// Called each time an event a CheckBox is ticked/unticked
		/// </summary>
		private void onCheck(object sender, EventArgs e) {
			started = false;
			updateActiveAdditionalContent();
			LoadPortraits();

			SetupSharedUI();
			SetupUI();
			started = true;

			DrawPortrait();
		}

		private void LoadPortraits() {
			object previouslySelectedPortrait = null;
			if (cbPortraitTypes.SelectedItem != null) {
				previouslySelectedPortrait = cbPortraitTypes.Items[cbPortraitTypes.SelectedIndex];
			}
			cbPortraitTypes.Items.Clear();

			loader.LoadPortraits();

			if (loader.activePortraitData.PortraitTypes.Count == 0) {
				logger.Fatal("No portrait types found.");
				return;
			}

			foreach (KeyValuePair<string, PortraitType> pair in loader.activePortraitData.PortraitTypes) {
				PortraitType portraitType = pair.Value;
				cbPortraitTypes.Items.Add(portraitType.Name.Replace("PORTRAIT_", ""));
			}

			if (previouslySelectedPortrait != null) {
				cbPortraitTypes.SelectedIndex = cbPortraitTypes.Items.IndexOf(previouslySelectedPortrait);
			}
			if (cbPortraitTypes.SelectedIndex == -1) {
				cbPortraitTypes.SelectedIndex = 0;
			}
		}

		private void updateActiveAdditionalContent() {
			List<Content> activeContent = new List<Content>();
			foreach (Control control in panelDLCs.Controls) {
				CheckBox checkbox = (CheckBox)control;
				if (checkbox.Checked) {
					activeContent.Add(usableMods[checkbox]);
				}
			}
			loader.UpdateActivateAdditionalContent(activeContent);
		}

		private void cbPortraitTypes_SelectedIndexChanged(object sender, EventArgs e) {
			if (started) {
				started = false;
				SetupUI();

				UpdateInputsFromPortraitData(portrait);

				started = true;

				DrawPortrait();
			}
		}

		private void UpdateInputsFromPortraitData(Portrait portrait) {
			for (int i = 0; i < dnaComboBoxes.Count; i++) {
				if (dnaComboBoxes[i] != null) {
					dnaComboBoxes[i].SelectedIndex = Portrait.GetIndex(portrait.GetDNA()[i], dnaComboBoxes[i].Items.Count);
				}
			}

			for (int i = 0; i < propertiesComboBoxes.Count; i++) {
				if (propertiesComboBoxes[i] != null) {
					propertiesComboBoxes[i].SelectedIndex = Portrait.GetIndex(portrait.GetProperties()[i], propertiesComboBoxes[i].Items.Count);
				}
			}
		}

		private void UpdatePortraitDataFromInputs() {
			portrait.import(GetDNA(), GetProperties());
			OutputDNA();
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e) {

		}
	}
}
