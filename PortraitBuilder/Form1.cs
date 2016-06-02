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
using Parsers.DLC;
using Parsers.Mod;
using Parsers.Portrait;

namespace Portrait_Builder {
	public partial class Form1 : Form {
		private Image previewImage = new Bitmap(176, 176);
		private bool started = false;
		private StringBuilder dnaPropOutput;
		public static Random rand = new Random();

		private string ck2Dir = string.Empty;
		private string myDocsDir = string.Empty;
		private string dlcDir = string.Empty;

		private Mod selectedMod = new Mod();
		private List<Mod> usableMods = new List<Mod>();
		private PortraitReader portraitReader = new PortraitReader();
		private List<Bitmap> borders = new List<Bitmap>();
		private string dna, properties;

		private StreamWriter log;
		private MemoryStream memoryStream;
		private bool forceLog = false;
		private bool fullLogging = false;
		private string version = string.Empty;
		private bool hadError = false;
		private StringBuilder modListSetup;


		public Form1() {
			InitializeComponent();
			EnvironmentSetup();
		}

		public Form1(bool logging, bool full) {
			InitializeComponent();

			forceLog = logging;
			fullLogging = full;

			EnvironmentSetup();
		}

		private void EnvironmentSetup() {
			myDocsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
										@"\Paradox Interactive\Crusader Kings II\";
			dlcDir = Environment.CurrentDirectory + @"\dlc";
			ReadDir();
			modListSetup = new StringBuilder();
			LoadDLC();
			LoadMods();

			Assembly assembly = Assembly.GetExecutingAssembly();
			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
			version = fvi.FileVersion;

			SetupStream();

			LoadPortraits();
			LoadBorders();

			if (hadError)
				return;

			SetupSharedUI();
			SetupUI();
			RandomizeUI(true);
			UpdatePortrait();

			started = true;
		}

		private void SetupStream() {
			memoryStream = new MemoryStream();
			log = new StreamWriter(memoryStream);

			if (fullLogging)
				portraitReader.SetLogging(log);

			Log("Portrait Builder Version " + version);
			Log("Portrait Builder Parser Library " + Parsers.Version.GetVersion());
			Log("Logging started at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			Log("");

			Log(modListSetup.ToString());
		}

		private void DumpLog() {
			Stream stream = new FileStream("log.txt", FileMode.Create, FileAccess.Write);
			memoryStream.WriteTo(stream);
			stream.Close();
			stream.Dispose();
		}

		private void Log(string text) {
			System.Diagnostics.Debug.WriteLine(text);
			log.WriteLine(text);
			log.Flush();
		}

		private void LoadBorders() {
			Log("Setting up borders.");
			if (!File.Exists(ck2Dir + @"\gfx\interface\charframe_150.dds")) {
				Log("Error: Borders file \\gfx\\interface\\charframe_150.dds not found.");
				hadError = true;
				DumpLog();

				MessageBox.Show(this, "Unable to find borders graphic.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			Bitmap charFrame = DevIL.DevIL.LoadBitmap(ck2Dir + @"\gfx\interface\charframe_150.dds");

			for (int i = 0; i < 6; i++) {
				Bitmap border = new Bitmap(176, 176);
				Graphics g = Graphics.FromImage(border);
				g.DrawImage(charFrame, 0, 0, new Rectangle(i * 176, 0, 176, 176), GraphicsUnit.Pixel);
				g.Dispose();
				borders.Add(border);
			}
		}

		private void LoadPortraits() {
			Log("Disposing of previous portrait data.");
			portraitReader.Dispose();

			if (cbModEnable.Checked &&
					selectedMod.CustomFlags.ContainsKey("HasPortrait") &&
					(bool)selectedMod.CustomFlags["HasPortrait"]) {
				Log("Loading portraits from mod: " + selectedMod.Name);

				string dir = string.Empty;
				switch (selectedMod.ModPathType) {
					case ModReader.Folder.CKDir:
						dir = ck2Dir;
						break;
					case ModReader.Folder.MyDocs:
						dir = myDocsDir;
						break;
					case ModReader.Folder.DLC:
						dir = dlcDir;
						break;
				}

				string[] fileNames = Directory.GetFiles(dir + @"\" + selectedMod.Path + @"\interface\", "*.gfx");

				foreach (string fileName in fileNames)
					portraitReader.Parse(fileName);
			} else {
				Log("Loading portraits from vanilla.");

				string[] fileNames = Directory.GetFiles(ck2Dir + @"\interface\", "*.gfx");

				foreach (string fileName in fileNames)
					portraitReader.Parse(fileName);
			}

			Log("The following errors were encountered while loading.");

			foreach (string error in portraitReader.Errors)
				Log(" --" + error);

			cbPortraitTypes.Items.Clear();

			if (portraitReader.PortraitTypes.Count == 0) {
				Log("Error: No portrait types found.");

				MessageBox.Show(this, "No portraits found in mod " + selectedMod.Name + "\n\nCheck errorlog.txt for details.", "Error", MessageBoxButtons.OK,
												 MessageBoxIcon.Error);

				DumpLog();

				hadError = true;

				return;
			}

			Log("Setting up type flags");
			foreach (KeyValuePair<string, PortraitType> pair in portraitReader.PortraitTypes) {
				Log(" --Setting up flags for " + pair.Value.Name);
				cbPortraitTypes.Items.Add(pair.Value.Name);

				foreach (Layer layer in pair.Value.Layers) {
					// Shared
					setupFlag(pair, layer, "background");
					setupFlag(pair, layer, "boils");
					setupFlag(pair, layer, "reddots");
					setupFlag(pair, layer, "scars");
					setupFlag(pair, layer, "imprisoned");
					setupFlag(pair, layer, "blinded");

					// Properties
					setupFlag(pair, layer, "clothes");
					setupFlag(pair, layer, "headgear");
					setupFlag(pair, layer, "beard");
					setupFlag(pair, layer, "hair");

					// DNA
					setupFlag(pair, layer, "base");
					setupFlag(pair, layer, "neck");
					setupFlag(pair, layer, "cheeks");
					setupFlag(pair, layer, "chin");
					setupFlag(pair, layer, "mouth");
					setupFlag(pair, layer, "nose");
					setupFlag(pair, layer, "eyes");
					setupFlag(pair, layer, "ear");
				}
			}
			cbPortraitTypes.SelectedIndex = 0;
		}

		private void setupFlag(KeyValuePair<string, PortraitType> pair, Layer layer, string layerName) {
			if (layer.Name.Contains(layerName) && !pair.Value.CustomFlags.ContainsKey(layerName))
				pair.Value.CustomFlags.Add(layerName, layer.Name);
		}

		private void LoadMods() {
			ModReader modReader = new ModReader();
			modListSetup.AppendLine("Loading mods from " + ck2Dir + @"\mod\");
			modReader.ParseFolder(ck2Dir + @"\mod\", ModReader.Folder.CKDir);
			modListSetup.AppendLine("Loading mods from " + myDocsDir + @"\mod\");
			modReader.ParseFolder(myDocsDir + @"\mod\", ModReader.Folder.MyDocs);

			foreach (Mod mod in modReader.Mods) {
				string dir = mod.ModPathType == ModReader.Folder.CKDir ? ck2Dir : myDocsDir;

				if (!Directory.Exists(dir + mod.Path))
					continue;

				if (File.Exists(dir + mod.Path + @"\interface\portraits.gfx")) {
					mod.CustomFlags.Add("HasPortrait", true);
					usableMods.Add(mod);
					continue;
				}

				if (Directory.Exists(dir + mod.Path + @"\gfx\characters\")) {
					if (Directory.GetDirectories(dir + mod.Path + @"\gfx\characters\").Length > 0) {
						mod.CustomFlags.Add("HasPortrait", false);
						usableMods.Add(mod);
					}
				}
			}

			modListSetup.AppendLine(" --Usable mods found:");
			if (usableMods.Count > 0) {
				cbModEnable.Enabled = true;
				foreach (Mod mod in usableMods) {
					modListSetup.AppendLine("   --" + mod.Name);
					cbMods.Items.Add(mod.Name);
					cbMods.SelectedIndex = 0;
				}
			}
		}

		private void LoadDLC() {
			DLCReader dlcReader = new DLCReader();
			modListSetup.AppendLine("Loading DLC from " + ck2Dir + @"\dlc\");
			dlcReader.ParseFolder(ck2Dir + @"\dlc");

			FastZip fastZip = new FastZip();
			foreach (DLC dlc in dlcReader.DLCs)
				fastZip.ExtractZip(ck2Dir + dlc.Archive, dlcDir, null);

			if (!Directory.Exists(dlcDir + @"\gfx\characters\"))
				return;

			Mod mod = new Mod();
			mod.ModPathType = ModReader.Folder.DLC;
			mod.Name = "DLC Portraits";
			mod.Path = string.Empty;
			mod.CustomFlags.Add("HasPortrait", true);
			usableMods.Add(mod);
		}

		private void ReadDir() {
			Stream stream = new FileStream("gamedir", FileMode.Open);
			BinaryReader reader = new BinaryReader(stream);
			{
				ck2Dir = reader.ReadString() + @"\";
			}
		}

		private void UpdatePortrait() {
			Log("Updating portrait.");

			GetDNA();
			GetProperties();
			DrawPortrait();
			OutputDNA();
		}

		private void DrawPortrait() {
			Log(" --Drawing portrait.");

			Graphics g = Graphics.FromImage(previewImage);

			Log("   --Clearing preview.");
			g.Clear(Color.Empty);
			Bitmap portrait;
			Log("   --Rendering portrait.");
			try {
				if (cbModEnable.Checked) {
					portrait = portraitReader.DrawPortrait(ck2Dir,
																			selectedMod,
																			portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()],
																			dna,
																			properties,
																			myDocsDir);
				} else {
					portrait = portraitReader.DrawPortrait(ck2Dir,
																			portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()],
																			dna,
																			properties,
																			myDocsDir);
				}
			} catch (Exception e) {
				Log("Error encountered rendering portrait:");
				Log(e.ToString());

				MessageBox.Show(this, "Error rendering portrait:\n\n" + e, "Error", MessageBoxButtons.OK,
												 MessageBoxIcon.Error);

				return;
			}
			g.DrawImage(portrait, 0, 0);
			Log("   --Drawing border.");
			g.DrawImage(borders[cbRank.SelectedIndex], 0, 0);

			if (portraitReader.DrawErrors.Count > 0) {
				Log("Errors encountered while rendering portrait:");
				foreach (string error in portraitReader.DrawErrors)
					Log(" --" + error);
			}

			pbPortrait.Image = previewImage;
		}

		private void GetDNA() {
			Log(" --Building DNA string.");
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

			dna = sb.ToString();
		}

		private void GetProperties() {
			Log(" --Building Properties string.");
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
			sb.Append("0"); // Player overlay

			properties = sb.ToString();
		}

		private void OutputDNA() {
			Log(" --Outputting DNA and Property strings.");
			dnaPropOutput = new StringBuilder();

			dnaPropOutput.Append("  dna=\"");
			dnaPropOutput.Append(dna);
			dnaPropOutput.AppendLine("\"");

			dnaPropOutput.Append("  properties=\"");
			dnaPropOutput.Append(properties);
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
			Log("Randomizing UI");
			if (doRank) {
				RandomizeComboBox(cbRank);
			}

			RandomizeComboBox(cbBackground);
			RandomizeComboBox(cbScars);

			cbRedDots.SelectedIndex = 0;
			cbBoils.SelectedIndex = 0;
			cbPrisoner.SelectedIndex = 0;
			if (cbBlinded.Items.Count > 0) {
				cbBlinded.SelectedIndex = 0;
				cbBlinded.Enabled = true;
			} else {
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
		}

		private void RandomizeComboBox(ComboBox cb) {
			cb.SelectedIndex = rand.Next(cb.Items.Count - 1);
		}

		private void FillComboBox(ComboBox cb, int count) {
			for (int i = 0; i < count; i++)
				cb.Items.Add(i);
		}

		private void FillComboBox(ComboBox cb, string flagName) {
			cb.Items.Clear();

			PortraitType portraitType = portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()];
			if (portraitType.CustomFlags.ContainsKey(flagName)) {
				string spriteName = (string)portraitType.CustomFlags[flagName];
				Sprite sprite = portraitReader.Sprites[spriteName];
				Log(" --" + flagName + " item count: " + sprite.FrameCount);
				FillComboBox(cb, sprite.FrameCount);
			} else {
				Log(" --No " + flagName + " flag found, setting UI to 26.");
				FillComboBox(cb, 26);
			}
		}

		private void SetupSharedUI() {
			Log("Setting up Shared UI");

			FillComboBox(cbBackground, "background");
			FillComboBox(cbScars, "scars");
			FillComboBox(cbRedDots, "reddots");
			FillComboBox(cbBoils, "boils");
			FillComboBox(cbPrisoner, "imprisoned");
			FillComboBox(cbBlinded, "blinded");
		}

		private void SetupUI() {
			PortraitType portraitType = portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()];

			Log("Setting up UI for: " + portraitType.Name);
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

			Log(" --Setting hair colours: " + portraitType.HairColours.Count);
			cbHairColour.Items.Clear();
			for (int i = 0; i < portraitType.HairColours.Count; i++) {
				cbHairColour.Items.Add(i);
			}

			Log(" --Setting eye colours: " + portraitType.EyeColours.Count);
			cbEyeColour.Items.Clear();
			for (int i = 0; i < portraitType.EyeColours.Count; i++) {
				cbEyeColour.Items.Add(i);
			}
		}

		private void cb_SelectedIndexChanged(object sender, EventArgs e) {
			if (started) {
				UpdatePortrait();
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

				cbNeck.SelectedIndex = GetIndex(dialog.Neck, cbNeck);
				cbChin.SelectedIndex = GetIndex(dialog.Chin, cbChin);
				cbMouth.SelectedIndex = GetIndex(dialog.Mouth, cbMouth);
				cbNose.SelectedIndex = GetIndex(dialog.Nose, cbNose);
				cbCheeks.SelectedIndex = GetIndex(dialog.Cheeks, cbCheeks);
				cbEyes.SelectedIndex = GetIndex(dialog.Eyes, cbEyes);
				cbEars.SelectedIndex = GetIndex(dialog.Ears, cbEars);
				cbHairColour.SelectedIndex = GetIndex(dialog.HairColour, cbHairColour);
				cbEyeColour.SelectedIndex = GetIndex(dialog.EyeColour, cbEyeColour);

				cbBackground.SelectedIndex = GetIndex(dialog.Background, cbBackground);
				cbHair.SelectedIndex = GetIndex(dialog.Hair, cbHair);
				cbClothes.SelectedIndex = GetIndex(dialog.Clothes, cbClothes);
				cbBeard.SelectedIndex = GetIndex(dialog.Beard, cbBeard);
				cbHeadgear.SelectedIndex = GetIndex(dialog.Headgear, cbHeadgear);
				cbPrisoner.SelectedIndex = GetIndex(dialog.Prison, cbPrisoner);
				cbScars.SelectedIndex = GetIndex(dialog.Scars, cbScars);
				cbRedDots.SelectedIndex = GetIndex(dialog.RedDots, cbRedDots);
				cbBoils.SelectedIndex = GetIndex(dialog.Boils, cbBoils);
				cbBlinded.SelectedIndex = GetIndex(dialog.Blinded, cbBlinded);

				started = true;

				UpdatePortrait();
			}
		}

		private int GetIndex(char letter, ComboBox cb) {
			int index = 0;

			if (cb.Items.Count == 0)
				return -1;

			if (letter == '0')
				return index;

			index = portraitReader.Letters.IndexOf(letter) + 1;
			index = index % cb.Items.Count;

			if (index == cb.Items.Count)
				index = 0;

			return index;
		}

		private void btnRandom_Click(object sender, EventArgs e) {
			started = false;
			RandomizeUI(false);
			started = true;

			UpdatePortrait();
		}

		private void cbMods_SelectedIndexChanged(object sender, EventArgs e) {
			selectedMod = usableMods[cbMods.SelectedIndex];

			if (started) {
				SetupStream();

				started = false;
				LoadPortraits();

				if (hadError)
					return;

				SetupSharedUI();
				SetupUI();
				RandomizeUI(false);
				started = true;

				UpdatePortrait();
			}
		}

		private void cbModEnable_CheckedChanged(object sender, EventArgs e) {
			cbMods.Enabled = cbModEnable.Checked;

			SetupStream();

			started = false;
			LoadPortraits();

			if (hadError)
				return;

			SetupSharedUI();
			SetupUI();
			RandomizeUI(false);
			started = true;

			UpdatePortrait();
		}

		private void cbPortraitTypes_SelectedIndexChanged(object sender, EventArgs e) {
			if (started) {
				started = false;
				SetupUI();

				cbNeck.SelectedIndex = GetIndex(dna[0], cbNeck);
				cbChin.SelectedIndex = GetIndex(dna[1], cbChin);
				cbMouth.SelectedIndex = GetIndex(dna[2], cbMouth);
				cbNose.SelectedIndex = GetIndex(dna[3], cbNose);
				cbCheeks.SelectedIndex = GetIndex(dna[4], cbCheeks);
				cbEyes.SelectedIndex = GetIndex(dna[6], cbEyes);
				cbEars.SelectedIndex = GetIndex(dna[7], cbEars);
				cbHairColour.SelectedIndex = GetIndex(dna[8], cbHairColour);
				cbEyeColour.SelectedIndex = GetIndex(dna[9], cbEyeColour);

				cbBackground.SelectedIndex = GetIndex(properties[0], cbBackground);
				cbHair.SelectedIndex = GetIndex(properties[1], cbHair);
				cbClothes.SelectedIndex = GetIndex(properties[3], cbClothes);
				cbBeard.SelectedIndex = GetIndex(properties[4], cbBeard);
				cbHeadgear.SelectedIndex = GetIndex(properties[5], cbHeadgear);
				cbPrisoner.SelectedIndex = GetIndex(properties[6], cbPrisoner);
				cbScars.SelectedIndex = GetIndex(properties[7], cbScars);
				cbRedDots.SelectedIndex = GetIndex(properties[8], cbRedDots);
				cbBoils.SelectedIndex = GetIndex(properties[9], cbBoils);

				started = true;

				UpdatePortrait();
			}
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
			if (!hadError && forceLog)
				DumpLog();
		}
	}
}
