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
	public partial class Form1 : Form {

		private static readonly ILog logger = LogManager.GetLogger(typeof(Form1).Name);

		private Image previewImage = new Bitmap(176, 176);
		private List<Bitmap> borders = new List<Bitmap>();

		private bool started = false;
		private StringBuilder dnaPropOutput;
		public static Random rand = new Random();

		/// <summary>
		/// User configuration: game path, etc.
		/// </summary>
		User user = new User();

		/// <summary>
		/// DLCs or Mods that are checked
		/// </summary>
		private List<AdditionalContent> activeMods = new List<AdditionalContent>();

		/// <summary>
		/// List of all available DLCs and Mods, indexed by their corresponding checkbox
		/// </summary>
		private Dictionary<CheckBox, AdditionalContent> usableMods = new Dictionary<CheckBox, AdditionalContent>();

		/// <summary>
		/// Stateless mod scanner
		/// </summary>
		private ModReader modReader = new ModReader();

		/// <summary>
		/// Stateless dlc scanner
		/// </summary>
		private DLCReader dlcReader = new DLCReader();

		/// <summary>
		/// TODO make it stateless
		/// </summary>
		private PortraitReader portraitReader = new PortraitReader();

		/// <summary>
		/// TODO make it stateless
		/// </summary>
		private PortraitOffsetReader portraitOffsetReader = new PortraitOffsetReader();

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

			user.GameDir = ReadGameDir();
			user.MyDocsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Paradox Interactive\Crusader Kings II\";
			user.DlcDir = Environment.CurrentDirectory + @"\dlc\";

			// Cleanup temporary DLC Dir
			Directory.Delete(user.DlcDir, true);

			logger.Info("Configuration: " + user);
			logger.Info("----------------------------");

			// Add the version to title
			this.Text += " " + Application.ProductVersion;

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
			logger.Debug("Setting up borders.");
			string borderSprite = user.GameDir + @"\gfx\interface\charframe_150.dds";

			if (!File.Exists(borderSprite)) {
				logger.Error("Borders file \\gfx\\interface\\charframe_150.dds not found.");
				return;
			}

			Bitmap charFrame = DevIL.DevIL.LoadBitmap(borderSprite);

			for (int i = 0; i < 6; i++) {
				Bitmap border = new Bitmap(176, 176);
				Graphics g = Graphics.FromImage(border);
				g.DrawImage(charFrame, 0, 0, new Rectangle(i * 176, 0, 176, 176), GraphicsUnit.Pixel);
				g.Dispose();
				borders.Add(border);
			}
		}

		private void LoadPortraits() {
			logger.Info("Disposing of previous portrait data.");
			portraitReader.Dispose();

			logger.Info("Loading portraits from vanilla.");
			LoadPortraitsFromDir(user.GameDir);


			foreach (AdditionalContent mod in activeMods) {
				if (mod.HasPortraits) {
					logger.Info("Loading portraits from mod: " + mod.Name);
					LoadPortraitsFromDir(mod.AbsolutePath);
				}
			}

			object previouslySelectedPortrait = null;
			if (cbPortraitTypes.SelectedItem != null) {
				previouslySelectedPortrait = cbPortraitTypes.Items[cbPortraitTypes.SelectedIndex];
			}
			cbPortraitTypes.Items.Clear();

			if (portraitReader.PortraitTypes.Count == 0) {
				logger.Fatal("No portrait types found.");
				return;
			}

			logger.Debug("Setting up type flags");
			foreach (KeyValuePair<string, PortraitType> pair in portraitReader.PortraitTypes) {
				PortraitType portraitType = pair.Value;
				logger.Debug(" --Setting up flags for " + portraitType.Name);
				cbPortraitTypes.Items.Add(portraitType.Name.Replace("PORTRAIT_",""));

				foreach (Layer layer in portraitType.Layers) {

					if (portraitOffsetReader.Offsets.ContainsKey(layer.Name)) {
						layer.Offset = portraitOffsetReader.Offsets[layer.Name];
						logger.Debug(string.Format("Overriding offset of layer {0} to {1}", layer.Name, layer.Offset));
					}

					setupFlags(portraitType, layer);
				}
			}

			if (previouslySelectedPortrait != null) {
				cbPortraitTypes.SelectedIndex = cbPortraitTypes.Items.IndexOf(previouslySelectedPortrait);
			}
			if(cbPortraitTypes.SelectedIndex == -1) {
				cbPortraitTypes.SelectedIndex = 0;
			}
		}

		private void LoadPortraitsFromDir(string dir) {
			List<string> fileNames = new List<string>();

			if(Directory.Exists(dir + @"\interface\")) {
				fileNames.AddRange(Directory.GetFiles(dir + @"\interface\", "*.gfx"));
			}
			if (Directory.Exists(dir + @"\interface\portraits\")){
				fileNames.AddRange(Directory.GetFiles(dir + @"\interface\portraits\", "*.gfx"));
			}

			foreach (string fileName in fileNames) {
				portraitReader.Parse(fileName);
			}

			if(Directory.Exists(dir + @"\interface\portrait_offsets\")) {
				string[] offsetFileNames = Directory.GetFiles(dir + @"\interface\portrait_offsets\", "*.txt");
				foreach (string offsetFileName in offsetFileNames) {
					portraitOffsetReader.Parse(offsetFileName);
				}
			}
		}

		// FIXME This should not be needed, as layer are associated via dna/properties letters already
		private void setupFlags(PortraitType portraitType, Layer layer) {
			// Shared
			setupFlag(portraitType, layer, "background");
			setupFlag(portraitType, layer, "boils");
			setupFlag(portraitType, layer, "reddots");
			setupFlag(portraitType, layer, "scars");
			setupFlag(portraitType, layer, "imprisoned");
			setupFlag(portraitType, layer, "blinded");

			// Properties
			setupFlag(portraitType, layer, "clothes");
			setupFlag(portraitType, layer, "headgear");
			setupFlag(portraitType, layer, "beard");
			setupFlag(portraitType, layer, "hair");

			// DNA
			setupFlag(portraitType, layer, "base");
			setupFlag(portraitType, layer, "neck");
			setupFlag(portraitType, layer, "cheeks");
			setupFlag(portraitType, layer, "chin");
			setupFlag(portraitType, layer, "mouth");
			setupFlag(portraitType, layer, "nose");
			setupFlag(portraitType, layer, "eyes");
			setupFlag(portraitType, layer, "ear");
		}

		private void setupFlag(PortraitType portraitType, Layer layer, string layerName) {
			if (layer.Name.Contains(layerName) && !portraitType.CustomFlags.ContainsKey(layerName))
				portraitType.CustomFlags.Add(layerName, layer.Name);
		}

		private void LoadMods() {
			logger.Info("Loading mods from " + user.MyDocsDir + @"\mod\");
			List<Mod> mods = modReader.ParseFolder(user.MyDocsDir + @"\mod\");

			foreach (Mod mod in mods) {

				if (!Directory.Exists(mod.AbsolutePath))
					continue;

				// FIXME also scan .gfx files
				if (Directory.Exists(mod.AbsolutePath + @"\gfx\characters\")) {
					mod.HasPortraits = false;
					registerMod(panelMods, mod);
				}
			}
		}

		private void LoadDLCs() {
			logger.Info("Loading DLCs from " + user.GameDir + @"dlc\");
			List<DLC> dlcs = dlcReader.ParseFolder(user.GameDir + @"\dlc");

			FastZip fastZip = new FastZip();
			foreach (DLC dlc in dlcs) {
				string dlcCode = dlc.DLCFile.Replace(".dlc", "");
				string newDlcAbsolutePath = user.DlcDir + dlcCode + @"\";
				logger.Info(string.Format("Extracting {0} to {1}", dlc.Name, newDlcAbsolutePath));
					
				// Filter only portraits files, to gain speed/space
				string fileFilter = @"interface;gfx/characters";
				fastZip.ExtractZip(dlc.AbsolutePath, newDlcAbsolutePath, fileFilter);
				dlc.AbsolutePath = newDlcAbsolutePath;

				// FIXME need to scan gfx too
				if (Directory.Exists(dlc.AbsolutePath + @"gfx\characters\")) {
					dlc.HasPortraits = true;
					registerMod(panelDLCs, dlc);
				}
			}
		}

		private void registerMod(Control container, AdditionalContent mod) {
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
				portraitImage = portraitReader.DrawPortrait(portraitType, portrait, activeMods, user);
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

		public char GetLetter(ComboBox cb) {
			char letter;

			if (cb.SelectedIndex == 0)
				letter = portraitReader.Letters[cb.Items.Count - 1];
			else if (cb.SelectedIndex == -1)
				letter = '0';
			else
				letter = portraitReader.Letters[cb.SelectedIndex - 1];

			return letter;
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
				Sprite sprite = portraitReader.Sprites[spriteName];
				logger.Debug(" --" + flagName + " item count: " + sprite.FrameCount);
				FillComboBox(cb, sprite.FrameCount);
			}
			else {
				logger.Warn(" --No " + flagName + " flag found, setting UI to 26.");
				FillComboBox(cb, 26);
			}
		}

		private PortraitType getSelectedPortraitType() {
			return portraitReader.PortraitTypes["PORTRAIT_" + cbPortraitTypes.SelectedItem.ToString()];
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

		private int GetIndex(char letter, int total) {
			if (total == 0)
				return -1;

			if (letter == '0')
				return 0;

			int index = (portraitReader.Letters.IndexOf(letter) + 1) % total;
			if (index == total) {
				index = 0;
			}
			return index;
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
			updateActiveMods();
			LoadPortraits();

			SetupSharedUI();
			SetupUI();
			started = true;

			DrawPortrait();
		}

		private void updateActiveMods() {
			activeMods = new List<AdditionalContent>();
			foreach (Control control in panelDLCs.Controls) {
				CheckBox checkbox = (CheckBox)control;
				if (checkbox.Checked) {
					activeMods.Add(usableMods[checkbox]);
				}
			}
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
					dnaComboBoxes[i].SelectedIndex = GetIndex(portrait.GetDNA()[i], dnaComboBoxes[i].Items.Count);
				}
			}

			for (int i = 0; i < propertiesComboBoxes.Count; i++) {
				if (propertiesComboBoxes[i] != null) {
					propertiesComboBoxes[i].SelectedIndex = GetIndex(portrait.GetProperties()[i], propertiesComboBoxes[i].Items.Count);
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
