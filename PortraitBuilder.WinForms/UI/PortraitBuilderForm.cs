using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
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

		private static readonly ILog logger = LogManager.GetLogger(typeof(PortraitBuilderForm));

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
		/// ComboBox for vanilla dna, ordered by their dna index.
		/// </summary>
		private Dictionary<Characteristic, ComboBox> dnaComboBoxes = new Dictionary<Characteristic, ComboBox>();

		/// <summary>
		/// ComboBox for vanilla properties, ordered by their properties index.
		/// </summary>
		private Dictionary<Characteristic, ComboBox> propertiesComboBoxes = new Dictionary<Characteristic, ComboBox>();

		/// <summary>
		/// ComboBox for custom mod properties, ordered by their properties index, and dynamically refreshed per portraitType.
		/// </summary>
		private Dictionary<Characteristic, ComboBox> customPropertiesComboBoxes = new Dictionary<Characteristic, ComboBox>();

		private ToolTip toolTip = new ToolTip();

		public PortraitBuilderForm() {
			InitializeComponent();

			foreach (Characteristic dna in Characteristic.DNA){
				registerCharacteristic(panelDNA, dna);
			}
			foreach (Characteristic property in Characteristic.PROPERTIES) {
				registerCharacteristic(panelProperties, property);
			}
			
			initializeForm();
			load(false);
			started = true;
		}

		private void initializeForm() {
			logger.Info("Portrait Builder Version " + Application.ProductVersion);
			// Add the version to title
			this.Text += " " + Application.ProductVersion;

			initializeTooltip();

			User user = new User();
			user.GameDir = readGameDir();
			user.ModDir = readModDir(user.GameDir);
			user.DlcDir = Path.Combine(Environment.CurrentDirectory, "dlc") + Path.DirectorySeparatorChar;
			logger.Info("Configuration: " + user);
			logger.Info("----------------------------");

			loader = new Loader(user);
		}

		private void initializeTooltip() {
			// Set up the delays for the ToolTip.
			toolTip.AutoPopDelay = 5000;
			toolTip.InitialDelay = 1000;
			toolTip.ReshowDelay = 500;
			// Force the ToolTip text to be displayed whether or not the form is active.
			toolTip.ShowAlways = true;

			// Set up the ToolTip text for form controls.
			toolTip.SetToolTip(this.btnToogleAll, "Check or uncheck checkboxes in active tab");
			toolTip.SetToolTip(this.btnReload, "Reload all data from folders");
			toolTip.SetToolTip(this.btnImport, "Import DNA and Properties strings");
			toolTip.SetToolTip(this.btnRandom, "Choose random values for dna/properties that are checked");
			toolTip.SetToolTip(this.btnSave, "Save portrait as a .png image");
			toolTip.SetToolTip(this.btnCopy, "Copy DNA & Properties to use for character history");

			toolTip.SetToolTip(this.cbPortraitTypes, "Select portraitType to render as base");
			toolTip.SetToolTip(this.cbCulturePortraitTypes, "Select portraitType to render for clothing override");
			toolTip.SetToolTip(this.cbRank, "Select rank to use for rendering portrait border");
			toolTip.SetToolTip(this.cbGovernment, "Select government to use for rendering. Theocracy and Merchant Republic use special sprites for headgear and clothing.");
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
				registerContent(panelMods, mod);
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

			if (content is Mod) {
				if (content.Enabled) {
					toolTip.SetToolTip(checkbox, "Toggle activation and file watching of this mod");
					content.Watcher = createModFilesWatcher(content);
				} else {
					// Note: can't use checkbox.Enabled since it disables tooltips as well !
					checkbox.ForeColor = Color.Gray; // Read-only appearance
					checkbox.AutoCheck = false; // Read-only behavior
					toolTip.SetToolTip(checkbox, content.DisabledReason);
				}
			} else {
				toolTip.SetToolTip(checkbox, "Toggle activation of this DLC");
			}
		}

		private void registerCharacteristic(Control container, Characteristic characteristic) {
			ComboBox combobox = new ComboBox();
			combobox.Width = 90;
			combobox.Padding = new Padding(0);
			combobox.Margin = new Padding(0);
			combobox.SelectedValueChanged += this.onChangeCharacteristic;

			Label label = new Label();
			label.Text = characteristic.ToString() + ":";
			label.Width = 90;
			label.TextAlign = ContentAlignment.MiddleRight;

			CheckBox randomizable = new CheckBox();
			randomizable.Width = 20;
			randomizable.Padding = new Padding(5, 0, 0, 0);
			randomizable.DataBindings.Add("Checked", characteristic, "Randomizable");
			toolTip.SetToolTip(randomizable, "Use this characteristic when randomizing");

			container.Controls.Add(label);
			container.Controls.Add(combobox);
			container.Controls.Add(randomizable);

			if (characteristic.type == Characteristic.Type.DNA) {
				dnaComboBoxes.Add(characteristic, combobox);
			} else if (characteristic.custom) {
				customPropertiesComboBoxes.Add(characteristic, combobox);
			} else {
				propertiesComboBoxes.Add(characteristic, combobox);
			}
		}

		private void unregisterCustomProperties() {
			for (int i = 0; i <= customPropertiesComboBoxes.Count*3 -1; i++) {
                // Remove Label + ComboBox + CheckBox for each custom property
                panelProperties.Controls.RemoveAt(panelProperties.Controls.Count -1);
			}
			customPropertiesComboBoxes.Clear();
		}

		private string readGameDir() {
			Stream stream = new FileStream("gamedir.txt", FileMode.Open);
            StreamReader reader = new StreamReader(stream);
            String gameDir = reader.ReadToEnd();
            logger.Info("Read gamedir: " + gameDir);
            return gameDir;
		}

		/// <summary>
		/// Read userdir.txt in Steam directory for the path to mod dir, or default to pre-defined location
		/// </summary>
		private string readModDir(string gameDir) {
			string userDir = getDefaultUserDir();
			string userdirFilePath = Path.Combine(gameDir, "userdir.txt");
			if(File.Exists(userdirFilePath)){
				logger.Info("Reading userdir.txt to determine the mod directory.");
				Stream stream = new FileStream(userdirFilePath, FileMode.Open);
				StreamReader reader = new StreamReader(stream, Encoding.Default);
                userDir = reader.ReadLine() + Path.DirectorySeparatorChar;
				logger.Info("Found userdir.txt with path: " + userDir);
			}
			return Path.Combine(userDir, "mod");
        }

        private string getDefaultUserDir() {
            string userDir = null;
            switch (OSUtils.determineOS()) {
                case OS.Windows:
                    userDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Paradox Interactive", "Crusader Kings II");
                    break;
                case OS.Mac:
                    // Environment.SpecialFolder.MyDocuments does not add /Documents/ on Mac
                    userDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Documents", "Paradox Interactive", "Crusader Kings II");
                    break;
                case OS.Linux:
                    userDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".paradoxinteractive", "Crusader Kings II");
                    break;
                case OS.Other:
                    logger.Error("Unkown operating system, cannot lookup mods mods, platformID:  " + System.Environment.OSVersion.Platform);
                    break;
            }
            return userDir;
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

		private string getCharacteristicsString(Dictionary<Characteristic, ComboBox> characteristics) {
			StringBuilder sb = new StringBuilder();
			foreach (ComboBox cb in characteristics.Values) {
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

			randomizeCharacteristics(dnaComboBoxes);
			randomizeCharacteristics(propertiesComboBoxes);
			randomizeCharacteristics(customPropertiesComboBoxes);

			updatePortrait(getCharacteristicsString(dnaComboBoxes), getCharacteristicsString(propertiesComboBoxes), getCharacteristicsString(customPropertiesComboBoxes));
		}

		private void randomizeCharacteristics(Dictionary<Characteristic, ComboBox> cbs) {
			foreach (KeyValuePair<Characteristic, ComboBox> pair in cbs) {
				if (pair.Key.Randomizable) {
					randomizeComboBox(pair.Value);
				}
			}
		}

		private void randomizeComboBox(ComboBox cb) {
			if (cb.Items.Count > 0) {
				cb.SelectedIndex = rand.Next(cb.Items.Count);
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
					if (frameCount == 1) {
						cb.Enabled = false; // Disable if only 1 frame, as there's no selection to do, for instance head (p2)
					}
				} else {
					logger.Warn(string.Format("Could not find frame count for {0} and {1}, disabling dropdown.", portraitType, characteristic));
					cb.Enabled = false;
				}
			}
		}

		private PortraitType getSelectedPortraitType() {
			PortraitType selectedPortraitType = null;
			object selectedItem = cbPortraitTypes.SelectedItem;
			object selectedItem2 = cbCulturePortraitTypes.SelectedItem;
			if (selectedItem != null) {
				if (selectedItem2 != null && !selectedItem2.ToString().Equals("")) {
					return loader.GetPortraitType("PORTRAIT_" + selectedItem.ToString(), "PORTRAIT_" + selectedItem2.ToString());
				} else {
					return loader.GetPortraitType("PORTRAIT_" + selectedItem.ToString());
				}
			}
			return selectedPortraitType;
		}

		private void fillCharacteristicComboBoxes() {
			fillCharacteristicComboBoxes(dnaComboBoxes);
			fillCharacteristicComboBoxes(propertiesComboBoxes);
			fillCharacteristicComboBoxes(customPropertiesComboBoxes);
		}

		private void fillCharacteristicComboBoxes(Dictionary<Characteristic, ComboBox> cbs) {
			foreach (KeyValuePair<Characteristic, ComboBox> pair in cbs) {
				ComboBox cb = pair.Value;
				if (cb != null) {
					fillComboBox(cb, pair.Key);
				}
			}
		}

		private void loadPortraitTypes() {
			object previouslySelectedBasePortrait = null;
			object previouslySelectedOverridePortrait = null;

			if (cbPortraitTypes.SelectedItem != null) {
				previouslySelectedBasePortrait = cbPortraitTypes.Items[cbPortraitTypes.SelectedIndex];
			}
			if (cbCulturePortraitTypes.SelectedItem != null) {
				previouslySelectedOverridePortrait = cbCulturePortraitTypes.Items[cbCulturePortraitTypes.SelectedIndex];
			}
			cbPortraitTypes.Items.Clear();
			cbCulturePortraitTypes.Items.Clear();

			loader.LoadPortraits();

			if (loader.GetActivePortraitData().PortraitTypes.Count == 0) {
				logger.Fatal("No portrait types found.");
				return;
			}

			cbCulturePortraitTypes.Items.Add(""); // Empty = no override
			foreach (KeyValuePair<string, PortraitType> pair in loader.GetActivePortraitData().PortraitTypes) {
				PortraitType portraitType = pair.Value;
				String portraitName = portraitType.Name.Replace("PORTRAIT_", "");
				if (portraitType.IsBasePortraitType()) {
					cbPortraitTypes.Items.Add(portraitName);
				}
				cbCulturePortraitTypes.Items.Add(portraitName);
			}

			if (previouslySelectedBasePortrait != null) {
				cbPortraitTypes.SelectedIndex = cbPortraitTypes.Items.IndexOf(previouslySelectedBasePortrait);
			}
			if (previouslySelectedOverridePortrait != null) {
				cbCulturePortraitTypes.SelectedIndex = cbCulturePortraitTypes.Items.IndexOf(previouslySelectedOverridePortrait);
			}
			if (cbPortraitTypes.SelectedIndex == -1) {
				cbPortraitTypes.SelectedIndex = 0;
			}
			if (cbCulturePortraitTypes.SelectedIndex == -1) {
				cbCulturePortraitTypes.SelectedIndex = 0;
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
			foreach (KeyValuePair<Characteristic, ComboBox> pair in dnaComboBoxes){
				if (pair.Value != null) {
					pair.Value.SelectedIndex = Portrait.GetIndex(portrait.GetDNA()[pair.Key.index], pair.Value.Items.Count);
				}
			}

			foreach (KeyValuePair<Characteristic, ComboBox> pair in propertiesComboBoxes){
				if (pair.Value != null) {
					pair.Value.SelectedIndex = Portrait.GetIndex(portrait.GetProperties()[pair.Key.index], pair.Value.Items.Count);
				}
			}
		}
		
		private void updatePortrait(string dna, string properties, string customProperties) {
			portrait.import(dna, properties + customProperties);
			outputDNA();
		}

		private FileSystemWatcher createModFilesWatcher(Content content) {
			FileSystemWatcher watcher = new FileSystemWatcher();
			
			watcher.Path = content.AbsolutePath;
			watcher.IncludeSubdirectories = true;

			// Do the filtering in event handlers, as watchers do not support Filter such as "*.gfx|*.txt|*.dds"
			watcher.Filter = "*.*";

			watcher.NotifyFilter = NotifyFilters.LastWrite;

			// Have the callbacks execute in Main thread
			watcher.SynchronizingObject = this;
			watcher.Changed += new FileSystemEventHandler(onContentFileChanged);
			watcher.Created += new FileSystemEventHandler(onContentFileChanged);
			watcher.Deleted += new FileSystemEventHandler(onContentFileChanged);
			watcher.Renamed += new RenamedEventHandler(onContentFileRenamed);

			watcher.Error += new ErrorEventHandler(onWatcherError);

			// Begin watching.
			watcher.EnableRaisingEvents = true;
			return watcher;
		}

		private Content getAssociatedContent(FileSystemWatcher watcher) {
			Content content = null;

			foreach (KeyValuePair<CheckBox, Content> pair in usableContents) {
				if (pair.Value.Watcher == watcher) {
					content = pair.Value;
					break;
				}
			}
			return content;
		}

		private void updateSelectedContent(List<CheckBox> cbs) {
			started = false;
			updateActiveAdditionalContent();
			loadPortraitTypes();
            refreshCustomCharacectiristics();

			fillCharacteristicComboBoxes();
			// TODO No refresh of DNA/Properties needed (if ComboBox has less options ?)
			started = true;

			foreach(CheckBox cb in cbs){
				Mod content = usableContents[cb] as Mod;
				if (content != null) {
					content.Watcher.EnableRaisingEvents = cb.Checked;
				}
			}

			drawPortrait();
		}

        private void refreshCustomCharacectiristics() {
            unregisterCustomProperties();
            foreach (Characteristic characteristic in getSelectedPortraitType().getCustomCharacterstics()) {
                if (!customPropertiesComboBoxes.ContainsKey(characteristic)) {
                    registerCharacteristic(panelProperties, characteristic);
                }
            }
        }

		///////////////////
		// Event handlers
		///////////////////

		private void onChangeCharacteristic(object sender, EventArgs e) {
			if (started) {
				// Assumption: customPropertiesComboBoxes are contiguous !
				updatePortrait(getCharacteristicsString(dnaComboBoxes), getCharacteristicsString(propertiesComboBoxes), getCharacteristicsString(customPropertiesComboBoxes));
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

				updatePortrait(dialog.portrait.GetDNA(), dialog.portrait.GetProperties(), "");

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
		/// Called each time a content CheckBox is ticked/unticked
		/// </summary>
		private void onCheckContent(object sender, EventArgs e) {
			CheckBox cb = (CheckBox)sender;
			List<CheckBox> cbs = new List<CheckBox>();
			cbs.Add(cb);
			updateSelectedContent(cbs);
		}

		private void onChangePortraitType(object sender, EventArgs e) {
			if (started) {
				started = false;

				PortraitType selectedPortraitType = getSelectedPortraitType();
				portrait.SetPortraitType(selectedPortraitType);

                refreshCustomCharacectiristics();

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
			List<CheckBox> cbs = new List<CheckBox>();
			foreach (CheckBox checkbox in tabPage.Controls[0].Controls) {
				// Remove handler so it doesn't trigger
				checkbox.CheckedChanged -= onCheckContent;
				checkbox.Checked = nextToogleIsSelectAll;
				checkbox.CheckedChanged += onCheckContent;
				cbs.Add(checkbox);
			}
			nextToogleIsSelectAll = !nextToogleIsSelectAll;
			updateSelectedContent(cbs);
		}

		private void onContentFileChanged(object sender, FileSystemEventArgs e) {
			FileSystemWatcher watcher = sender as FileSystemWatcher;
			onContentChange(watcher, e.FullPath);
		}

		private void onContentFileRenamed(object sender, RenamedEventArgs e) {
			FileSystemWatcher watcher = sender as FileSystemWatcher;
			onContentChange(watcher, e.OldFullPath);
		}

		private void onContentChange(FileSystemWatcher watcher, string path) {
			// Workaround same change firing multiple events
			watcher.EnableRaisingEvents = false;

			Content content = getAssociatedContent(watcher);

			if (content != null) {
				logger.Info(string.Format("Content change for {0} in content {1}", path, content));
				loader.RefreshContent(content);
				loadPortraitTypes();
				fillCharacteristicComboBoxes();
				drawPortrait();
			} else {
				logger.Error(string.Format("No content matched for watcher on file {0}", path));
			}

			watcher.EnableRaisingEvents = true;
		}

		private void onWatcherError(object sender, ErrorEventArgs e){
			logger.Error("FileSystemWatcher unable to continue", e.GetException());
		}
    }
}
