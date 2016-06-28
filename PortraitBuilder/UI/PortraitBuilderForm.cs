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
using PortraitBuilder.Engine;
using PortraitBuilder.Model.Content;
using PortraitBuilder.Model.Portrait;
using PortraitBuilder.Model;

namespace PortraitBuilder.UI {

	/// <summary>
	/// Controller class
	/// </summary>
	public partial class PortraitBuilderForm : Form {

		private static readonly ILog logger = LogManager.GetLogger(typeof(PortraitBuilderForm).Name);

		private Image previewImage = new Bitmap(176, 176);

		private bool started = false;
		public static Random rand = new Random();

		private Boolean nextToogleIsSelectAll = true;

		private Loader loader;

		private PortraitRenderer portraitRenderer = new PortraitRenderer();

		/// <summary>
		/// List of all available DLCs and Mods, indexed by their corresponding checkbox
		/// </summary>
		private Dictionary<CheckBox, Content> usableContents = new Dictionary<CheckBox, Content>();

		/// <summary>
		/// The portrait being previewed. 
		/// 
		/// This is the primary Model object, whose state is modified by UI inputs, and used to display the output.
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

		public PortraitBuilderForm() {
			InitializeComponent();
			dnaComboBoxes.AddRange(new ComboBox[] { cbNeck, cbChin, cbMouth, cbNose, cbCheeks, null, cbEyes, cbEars, cbHairColour, cbEyeColour, null });
			propertiesComboBoxes.AddRange(new ComboBox[] { cbBackground, cbHair, null, cbClothes, cbBeard, cbHeadgear, cbPrisoner, cbScars, cbRedDots, cbBoils, cbBlinded, cbPlayer });

			initialize();
			load(false);
			started = true;
		}

		private void initialize() {
			logger.Info("Portrait Builder Version " + Application.ProductVersion);
			// Add the version to title
			this.Text += " " + Application.ProductVersion;

			User user = new User();
			user.GameDir = readGameDir();
			user.MyDocsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Paradox Interactive\Crusader Kings II\";
			user.DlcDir = Environment.CurrentDirectory + @"\dlc\";
			logger.Info("Configuration: " + user);
			logger.Info("----------------------------");

			loader = new Loader(user);
		}

		private void load(bool clean) {
			logger.Info("----------------------------");
			logger.Info("(Re-)loading data");

			loader.LoadVanilla();
			loadDLCs(clean);
			loadMods();

			loadPortraitTypes();
			fillCharacteristicComboBoxes();
			randomizeCharacteristics(true);

			drawPortrait();
		}

		private void loadMods() {
			List<Mod> mods = loader.LoadMods();
			panelMods.Controls.Clear();
			foreach (Mod mod in mods) {
				if (mod.GetHasPortraitData()) {
					registerContent(panelMods, mod);
				}
			}
		}

		private void loadDLCs(bool clean) {
			List<DLC> dlcs = loader.LoadDLCs(clean);
			panelDLCs.Controls.Clear();
			foreach (DLC dlc in dlcs) {
				if (dlc.GetHasPortraitData()) {
					registerContent(panelDLCs, dlc);
				}
			}
		}

		private void registerContent(Control container, Content content) {
			CheckBox checkbox = new CheckBox();
			checkbox.Text = content.Name;
			checkbox.AutoEllipsis = true;
			checkbox.Width = 190; // Force overflow
			checkbox.CheckedChanged += this.onCheckContent;
			checkbox.Padding = new Padding(0);
			checkbox.Margin = new Padding(0);

			container.Controls.Add(checkbox);
			usableContents.Add(checkbox, content);
		}

		private string readGameDir() {
			Stream stream = new FileStream("gamedir", FileMode.Open);
			BinaryReader reader = new BinaryReader(stream);
			return reader.ReadString() + @"\";
		}

		/// <summary>
		/// Entry point for re-drawing based on updated portrait.
		/// </summary>
		private void drawPortrait() {
			Graphics g = Graphics.FromImage(previewImage);

			logger.Debug("Clearing preview.");
			g.Clear(Color.Empty);
			
			try {
				Bitmap portraitImage = portraitRenderer.DrawPortrait(portrait, loader.GetActiveContents(), loader.GetActivePortraitData().Sprites);
				g.DrawImage(portraitImage, 0, 0);
			} catch (Exception e) {
				logger.Error("Error encountered rendering portrait", e);
				return;
			}

			pbPortrait.Image = previewImage;
		}

		private string getCharacteristicsString(List<ComboBox> characteristics) {
			StringBuilder sb = new StringBuilder();
			foreach (ComboBox cb in characteristics) {
				char letter = '0';
				if (cb != null) {
					letter = GetLetter(cb);
				}
				sb.Append(letter);
			}
			return sb.ToString();
		}

		// Needs to be called each time portrait object is modified
		private void outputDNA() {
			logger.Debug(" --Outputting DNA and Property strings.");
			StringBuilder dnaPropOutput = new StringBuilder();

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

		/// <summary>
		/// Some very specific characristics are not randomized: scars, red dots, boils, prisoner, blinded.
		/// </summary>
		/// <param name="doRank"></param>
		private void randomizeCharacteristics(bool doRank) {
			logger.Debug("Randomizing UI");
			if (doRank) {
				randomizeComboBox(cbGovernment);
				randomizeComboBox(cbRank);
			}

			randomizeComboBox(cbBackground);
			randomizeComboBox(cbClothes);
			randomizeComboBox(cbHeadgear);
			randomizeComboBox(cbHair);
			randomizeComboBox(cbBeard);
			randomizeComboBox(cbNeck);
			randomizeComboBox(cbCheeks);
			randomizeComboBox(cbChin);
			randomizeComboBox(cbMouth);
			randomizeComboBox(cbNose);
			randomizeComboBox(cbEyes);
			randomizeComboBox(cbEars);
			randomizeComboBox(cbHairColour);
			randomizeComboBox(cbEyeColour);

			resetComboBox(cbScars);
			resetComboBox(cbRedDots);
			resetComboBox(cbBoils);
			resetComboBox(cbPrisoner);
			resetComboBox(cbBlinded);
			resetComboBox(cbPlayer);

			updatePortrait(getCharacteristicsString(dnaComboBoxes), getCharacteristicsString(propertiesComboBoxes));

		}

		private void randomizeComboBox(ComboBox cb) {
			if (cb.Items.Count > 0) {
				cb.SelectedIndex = rand.Next(cb.Items.Count - 1);
			}
		}

		private void resetComboBox(ComboBox cb) {
			if (cb.Items.Count > 0) {
				cb.SelectedIndex = 0;
			}
		}

		private void fillComboBox(ComboBox cb, int count) {
			for (int i = 0; i < count; i++)
				cb.Items.Add(i);
		}

		private void fillComboBox(ComboBox cb, Characteristic characteristic) {
			cb.Items.Clear();
			PortraitType portraitType = portrait.GetPortraitType();
			if (portraitType != null) {
				int frameCount = loader.GetActivePortraitData().GetFrameCount(portraitType, characteristic);
				if (frameCount > 0) {
					logger.Debug(string.Format("Item count for {0} {1} : {2}", portraitType, characteristic, frameCount));
					cb.Enabled = true;
					fillComboBox(cb, frameCount);
				} else {
					logger.Warn(string.Format("Could not find frame count for {0} and {1}, disabling dropdown.", portraitType, characteristic));
					cb.Enabled = false;
				}
			}
		}

		private PortraitType getSelectedPortraitType() {
			PortraitType selectedPortraitType = null;
			object selectedItem = cbPortraitTypes.SelectedItem;
			if (selectedItem != null) {
				return loader.GetActivePortraitData().PortraitTypes["PORTRAIT_" + selectedItem.ToString()];
			}
			return selectedPortraitType;
		}

		private void fillCharacteristicComboBoxes() {
			fillCharacteristicComboBoxes(dnaComboBoxes, Characteristic.DNA);
			fillCharacteristicComboBoxes(propertiesComboBoxes, Characteristic.PROPERTIES);
		}

		private void fillCharacteristicComboBoxes(List<ComboBox> cbs, Characteristic[] characteristics) {
			for (int i = 0; i < characteristics.Length; i++) {
				ComboBox cb = cbs[i];
				if (cb != null) {
					fillComboBox(cb, characteristics[i]);
				}
			}
		}

		private void loadPortraitTypes() {
			object previouslySelectedPortrait = null;
			if (cbPortraitTypes.SelectedItem != null) {
				previouslySelectedPortrait = cbPortraitTypes.Items[cbPortraitTypes.SelectedIndex];
			}
			cbPortraitTypes.Items.Clear();

			loader.LoadPortraits();

			if (loader.GetActivePortraitData().PortraitTypes.Count == 0) {
				logger.Fatal("No portrait types found.");
				return;
			}

			foreach (KeyValuePair<string, PortraitType> pair in loader.GetActivePortraitData().PortraitTypes) {
				PortraitType portraitType = pair.Value;
				cbPortraitTypes.Items.Add(portraitType.Name.Replace("PORTRAIT_", ""));
			}

			if (previouslySelectedPortrait != null) {
				cbPortraitTypes.SelectedIndex = cbPortraitTypes.Items.IndexOf(previouslySelectedPortrait);
			}
			if (cbPortraitTypes.SelectedIndex == -1) {
				cbPortraitTypes.SelectedIndex = 0;
			}
			portrait.SetPortraitType(getSelectedPortraitType());
		}

		private void updateActiveAdditionalContent() {
			List<Content> activeContent = new List<Content>();
			activeContent.AddRange(getSelectedContent(panelDLCs));
			activeContent.AddRange(getSelectedContent(panelMods));
			loader.UpdateActiveAdditionalContent(activeContent);
		}

		private List<Content> getSelectedContent(Panel panel) {
			List<Content> selectedContent = new List<Content>();
			foreach (Control control in panel.Controls) {
				CheckBox checkbox = (CheckBox)control;
				if (checkbox.Checked) {
					selectedContent.Add(usableContents[checkbox]);
				}
			}
			return selectedContent;
		}

		private void updateSelectedCharacteristicValues(Portrait portrait) {
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
		
		private void updatePortrait(string dna, string properties) {
			portrait.import(dna, properties);
			outputDNA();
		}

		// Event handlers

		private void onChangeCharacteristic(object sender, EventArgs e) {
			if (started) {
				updatePortrait(getCharacteristicsString(dnaComboBoxes), getCharacteristicsString(propertiesComboBoxes));
				drawPortrait();
			}
		}

		private void onChangeRank(object sender, EventArgs e) {
			portrait.SetRank(cbRank.SelectedIndex);
			drawPortrait();
		}

		private void onChangeGovernment(object sender, EventArgs e) {
			portrait.SetGovernment(cbGovernment.SelectedIndex);
			drawPortrait();
		}

		private void onClickCopy(object sender, EventArgs e) {
			Clipboard.SetText(tbDNA.Text);
		}

		private void onClickSave(object sender, EventArgs e) {
			string file = Snippets.SaveFileDialog("Save Image", "PNG|*.png", null);

			if (file != null) {
				previewImage.Save(file, ImageFormat.Png);
			}
		}

		private void onClickImport(object sender, EventArgs e) {
			ImportDialog dialog = new ImportDialog();

			if (dialog.ShowDialog(this) == DialogResult.OK) {
				started = false;

				updatePortrait(dialog.portrait.GetDNA(), dialog.portrait.GetProperties());

				// Reflect on dropdown
				updateSelectedCharacteristicValues(portrait);

				started = true;

				drawPortrait();
			}
		}

		private void onClickRandomize(object sender, EventArgs e) {
			started = false;
			randomizeCharacteristics(false);
			started = true;

			drawPortrait();
		}

		/// <summary>
		/// Called each time an event a CheckBox is ticked/unticked
		/// </summary>
		private void onCheckContent(object sender, EventArgs e) {
			started = false;
			updateActiveAdditionalContent();
			loadPortraitTypes();
			fillCharacteristicComboBoxes();
			// TODO No refresh of DNA/Properties needed (if ComboBox has less options ?)
			started = true;

			drawPortrait();
		}

		private void onChangePortraitType(object sender, EventArgs e) {
			if (started) {
				started = false;

				portrait.SetPortraitType(getSelectedPortraitType());
				fillCharacteristicComboBoxes();
				updateSelectedCharacteristicValues(portrait);

				started = true;

				drawPortrait();
			}
		}

		private void onClickReload(object sender, EventArgs e) {
			foreach (Content content in usableContents.Values) {
				content.Dispose();
			}
			usableContents.Clear();
			load(true);
		}

		private void onClickToogleAll(object sender, EventArgs e) {
			TabPage tabPage = tabContent.SelectedTab;
			foreach (CheckBox checkbox in tabPage.Controls[0].Controls) {
				// Remove handler so it doesn't trigger
				checkbox.CheckedChanged -= onCheckContent;
				checkbox.Checked = nextToogleIsSelectAll;
				checkbox.CheckedChanged += onCheckContent;
			}
			nextToogleIsSelectAll = !nextToogleIsSelectAll;
			onCheckContent(sender, e);
		}
	}
}
