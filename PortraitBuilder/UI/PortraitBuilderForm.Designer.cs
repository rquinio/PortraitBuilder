﻿using System;

namespace Portrait_Builder {
	partial class PortraitBuilderForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PortraitBuilderForm));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label22 = new System.Windows.Forms.Label();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnCopy = new System.Windows.Forms.Button();
			this.tbDNA = new System.Windows.Forms.TextBox();
			this.pbPortrait = new System.Windows.Forms.PictureBox();
			this.cbRank = new System.Windows.Forms.ComboBox();
			this.label20 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.label15 = new System.Windows.Forms.Label();
			this.cbEyeColour = new System.Windows.Forms.ComboBox();
			this.label11 = new System.Windows.Forms.Label();
			this.cbHairColour = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cbCheeks = new System.Windows.Forms.ComboBox();
			this.cbMouth = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cbEars = new System.Windows.Forms.ComboBox();
			this.cbChin = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.cbEyes = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.cbNose = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.cbNeck = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.label23 = new System.Windows.Forms.Label();
			this.cbPlayer = new System.Windows.Forms.ComboBox();
			this.label19 = new System.Windows.Forms.Label();
			this.cbBlinded = new System.Windows.Forms.ComboBox();
			this.label13 = new System.Windows.Forms.Label();
			this.cbBoils = new System.Windows.Forms.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.cbRedDots = new System.Windows.Forms.ComboBox();
			this.label16 = new System.Windows.Forms.Label();
			this.cbScars = new System.Windows.Forms.ComboBox();
			this.label17 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.cbPrisoner = new System.Windows.Forms.ComboBox();
			this.cbBackground = new System.Windows.Forms.ComboBox();
			this.cbClothes = new System.Windows.Forms.ComboBox();
			this.label8 = new System.Windows.Forms.Label();
			this.cbHeadgear = new System.Windows.Forms.ComboBox();
			this.label10 = new System.Windows.Forms.Label();
			this.cbBeard = new System.Windows.Forms.ComboBox();
			this.label12 = new System.Windows.Forms.Label();
			this.cbHair = new System.Windows.Forms.ComboBox();
			this.label14 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label21 = new System.Windows.Forms.Label();
			this.btnRandom = new System.Windows.Forms.Button();
			this.btnImport = new System.Windows.Forms.Button();
			this.cbPortraitTypes = new System.Windows.Forms.ComboBox();
			this.tabContent = new System.Windows.Forms.TabControl();
			this.tabDLCs = new System.Windows.Forms.TabPage();
			this.panelDLCs = new System.Windows.Forms.FlowLayoutPanel();
			this.tabMods = new System.Windows.Forms.TabPage();
			this.panelMods = new System.Windows.Forms.FlowLayoutPanel();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.btnToogleAll = new System.Windows.Forms.Button();
			this.btnReload = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbPortrait)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.tabContent.SuspendLayout();
			this.tabDLCs.SuspendLayout();
			this.tabMods.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label22);
			this.groupBox1.Controls.Add(this.btnSave);
			this.groupBox1.Controls.Add(this.btnCopy);
			this.groupBox1.Controls.Add(this.tbDNA);
			this.groupBox1.Controls.Add(this.pbPortrait);
			this.groupBox1.Controls.Add(this.cbRank);
			this.groupBox1.Controls.Add(this.label20);
			this.groupBox1.Location = new System.Drawing.Point(473, 10);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(188, 402);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Output";
			// 
			// label22
			// 
			this.label22.AutoSize = true;
			this.label22.Location = new System.Drawing.Point(2, 325);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(181, 26);
			this.label22.TabIndex = 24;
			this.label22.Text = "*Warning: properties are dynamically \r\noverridden by portrait_properties.txt";
			// 
			// btnSave
			// 
			this.btnSave.Location = new System.Drawing.Point(6, 375);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(75, 23);
			this.btnSave.TabIndex = 22;
			this.btnSave.Text = "Save Image";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.onClickSave);
			// 
			// btnCopy
			// 
			this.btnCopy.Location = new System.Drawing.Point(107, 375);
			this.btnCopy.Name = "btnCopy";
			this.btnCopy.Size = new System.Drawing.Size(75, 23);
			this.btnCopy.TabIndex = 23;
			this.btnCopy.Text = "Copy";
			this.btnCopy.UseVisualStyleBackColor = true;
			this.btnCopy.Click += new System.EventHandler(this.onClickCopy);
			// 
			// tbDNA
			// 
			this.tbDNA.Location = new System.Drawing.Point(6, 250);
			this.tbDNA.Multiline = true;
			this.tbDNA.Name = "tbDNA";
			this.tbDNA.ReadOnly = true;
			this.tbDNA.Size = new System.Drawing.Size(176, 34);
			this.tbDNA.TabIndex = 23;
			this.tbDNA.TabStop = false;
			this.tbDNA.Text = "dna\r\nproperties";
			// 
			// pbPortrait
			// 
			this.pbPortrait.Location = new System.Drawing.Point(6, 68);
			this.pbPortrait.Name = "pbPortrait";
			this.pbPortrait.Size = new System.Drawing.Size(176, 176);
			this.pbPortrait.TabIndex = 0;
			this.pbPortrait.TabStop = false;
			// 
			// cbRank
			// 
			this.cbRank.FormattingEnabled = true;
			this.cbRank.Items.AddRange(new object[] {
            "None",
            "Baron",
            "Count",
            "Duke",
            "King",
            "Emperor"});
			this.cbRank.Location = new System.Drawing.Point(45, 19);
			this.cbRank.Name = "cbRank";
			this.cbRank.Size = new System.Drawing.Size(137, 21);
			this.cbRank.TabIndex = 21;
			this.cbRank.SelectedIndexChanged += new System.EventHandler(this.onChangeRank);
			// 
			// label20
			// 
			this.label20.AutoSize = true;
			this.label20.Location = new System.Drawing.Point(6, 22);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(36, 13);
			this.label20.TabIndex = 2;
			this.label20.Text = "Rank:";
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Location = new System.Drawing.Point(6, 46);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(211, 327);
			this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
			this.tabControl1.TabIndex = 3;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.label15);
			this.tabPage1.Controls.Add(this.cbEyeColour);
			this.tabPage1.Controls.Add(this.label11);
			this.tabPage1.Controls.Add(this.cbHairColour);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.cbCheeks);
			this.tabPage1.Controls.Add(this.cbMouth);
			this.tabPage1.Controls.Add(this.label4);
			this.tabPage1.Controls.Add(this.cbEars);
			this.tabPage1.Controls.Add(this.cbChin);
			this.tabPage1.Controls.Add(this.label7);
			this.tabPage1.Controls.Add(this.label3);
			this.tabPage1.Controls.Add(this.cbEyes);
			this.tabPage1.Controls.Add(this.label6);
			this.tabPage1.Controls.Add(this.cbNose);
			this.tabPage1.Controls.Add(this.label5);
			this.tabPage1.Controls.Add(this.cbNeck);
			this.tabPage1.Controls.Add(this.label1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(203, 301);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "DNA";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(19, 225);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(82, 13);
			this.label15.TabIndex = 15;
			this.label15.Text = "Eye Colour (d9):";
			// 
			// cbEyeColour
			// 
			this.cbEyeColour.FormattingEnabled = true;
			this.cbEyeColour.Location = new System.Drawing.Point(107, 222);
			this.cbEyeColour.Name = "cbEyeColour";
			this.cbEyeColour.Size = new System.Drawing.Size(90, 21);
			this.cbEyeColour.TabIndex = 9;
			this.cbEyeColour.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(18, 198);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(83, 13);
			this.label11.TabIndex = 16;
			this.label11.Text = "Hair Colour (d8):";
			// 
			// cbHairColour
			// 
			this.cbHairColour.FormattingEnabled = true;
			this.cbHairColour.Location = new System.Drawing.Point(107, 195);
			this.cbHairColour.Name = "cbHairColour";
			this.cbHairColour.Size = new System.Drawing.Size(90, 21);
			this.cbHairColour.TabIndex = 8;
			this.cbHairColour.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(34, 117);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(67, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Cheeks (d4):";
			// 
			// cbCheeks
			// 
			this.cbCheeks.FormattingEnabled = true;
			this.cbCheeks.Location = new System.Drawing.Point(107, 114);
			this.cbCheeks.Name = "cbCheeks";
			this.cbCheeks.Size = new System.Drawing.Size(90, 21);
			this.cbCheeks.TabIndex = 5;
			this.cbCheeks.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// cbMouth
			// 
			this.cbMouth.FormattingEnabled = true;
			this.cbMouth.Location = new System.Drawing.Point(107, 60);
			this.cbMouth.Name = "cbMouth";
			this.cbMouth.Size = new System.Drawing.Size(90, 21);
			this.cbMouth.TabIndex = 7;
			this.cbMouth.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(40, 63);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(61, 13);
			this.label4.TabIndex = 0;
			this.label4.Text = "Mouth (d2):";
			// 
			// cbEars
			// 
			this.cbEars.FormattingEnabled = true;
			this.cbEars.Location = new System.Drawing.Point(107, 168);
			this.cbEars.Name = "cbEars";
			this.cbEars.Size = new System.Drawing.Size(90, 21);
			this.cbEars.TabIndex = 10;
			this.cbEars.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// cbChin
			// 
			this.cbChin.FormattingEnabled = true;
			this.cbChin.Location = new System.Drawing.Point(107, 33);
			this.cbChin.Name = "cbChin";
			this.cbChin.Size = new System.Drawing.Size(90, 21);
			this.cbChin.TabIndex = 6;
			this.cbChin.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(49, 171);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(52, 13);
			this.label7.TabIndex = 0;
			this.label7.Text = "Ears (d7):";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(49, 36);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(52, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "Chin (d1):";
			// 
			// cbEyes
			// 
			this.cbEyes.FormattingEnabled = true;
			this.cbEyes.Location = new System.Drawing.Point(107, 141);
			this.cbEyes.Name = "cbEyes";
			this.cbEyes.Size = new System.Drawing.Size(90, 21);
			this.cbEyes.TabIndex = 9;
			this.cbEyes.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(47, 144);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(54, 13);
			this.label6.TabIndex = 0;
			this.label6.Text = "Eyes (d6):";
			// 
			// cbNose
			// 
			this.cbNose.FormattingEnabled = true;
			this.cbNose.Location = new System.Drawing.Point(107, 87);
			this.cbNose.Name = "cbNose";
			this.cbNose.Size = new System.Drawing.Size(90, 21);
			this.cbNose.TabIndex = 8;
			this.cbNose.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(45, 90);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(56, 13);
			this.label5.TabIndex = 0;
			this.label5.Text = "Nose (d3):";
			// 
			// cbNeck
			// 
			this.cbNeck.FormattingEnabled = true;
			this.cbNeck.Location = new System.Drawing.Point(107, 6);
			this.cbNeck.Name = "cbNeck";
			this.cbNeck.Size = new System.Drawing.Size(90, 21);
			this.cbNeck.TabIndex = 4;
			this.cbNeck.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(44, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Neck (d0):";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.label23);
			this.tabPage2.Controls.Add(this.cbPlayer);
			this.tabPage2.Controls.Add(this.label19);
			this.tabPage2.Controls.Add(this.cbBlinded);
			this.tabPage2.Controls.Add(this.label13);
			this.tabPage2.Controls.Add(this.cbBoils);
			this.tabPage2.Controls.Add(this.label9);
			this.tabPage2.Controls.Add(this.cbRedDots);
			this.tabPage2.Controls.Add(this.label16);
			this.tabPage2.Controls.Add(this.cbScars);
			this.tabPage2.Controls.Add(this.label17);
			this.tabPage2.Controls.Add(this.label18);
			this.tabPage2.Controls.Add(this.cbPrisoner);
			this.tabPage2.Controls.Add(this.cbBackground);
			this.tabPage2.Controls.Add(this.cbClothes);
			this.tabPage2.Controls.Add(this.label8);
			this.tabPage2.Controls.Add(this.cbHeadgear);
			this.tabPage2.Controls.Add(this.label10);
			this.tabPage2.Controls.Add(this.cbBeard);
			this.tabPage2.Controls.Add(this.label12);
			this.tabPage2.Controls.Add(this.cbHair);
			this.tabPage2.Controls.Add(this.label14);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(203, 301);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Properties*";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// label23
			// 
			this.label23.AutoSize = true;
			this.label23.Location = new System.Drawing.Point(35, 279);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(66, 13);
			this.label23.TabIndex = 10;
			this.label23.Text = "Player (p11):";
			// 
			// cbPlayer
			// 
			this.cbPlayer.FormattingEnabled = true;
			this.cbPlayer.Location = new System.Drawing.Point(107, 276);
			this.cbPlayer.Name = "cbPlayer";
			this.cbPlayer.Size = new System.Drawing.Size(90, 21);
			this.cbPlayer.TabIndex = 9;
			this.cbPlayer.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(29, 252);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(72, 13);
			this.label19.TabIndex = 2;
			this.label19.Text = "Blinded (p10):";
			// 
			// cbBlinded
			// 
			this.cbBlinded.FormattingEnabled = true;
			this.cbBlinded.Location = new System.Drawing.Point(107, 249);
			this.cbBlinded.Name = "cbBlinded";
			this.cbBlinded.Size = new System.Drawing.Size(90, 21);
			this.cbBlinded.TabIndex = 8;
			this.cbBlinded.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(48, 225);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(53, 13);
			this.label13.TabIndex = 3;
			this.label13.Text = "Boils (p9):";
			// 
			// cbBoils
			// 
			this.cbBoils.FormattingEnabled = true;
			this.cbBoils.Location = new System.Drawing.Point(107, 222);
			this.cbBoils.Name = "cbBoils";
			this.cbBoils.Size = new System.Drawing.Size(90, 21);
			this.cbBoils.TabIndex = 6;
			this.cbBoils.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(25, 198);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(76, 13);
			this.label9.TabIndex = 7;
			this.label9.Text = "Red Dots (p8):";
			// 
			// cbRedDots
			// 
			this.cbRedDots.FormattingEnabled = true;
			this.cbRedDots.Location = new System.Drawing.Point(107, 195);
			this.cbRedDots.Name = "cbRedDots";
			this.cbRedDots.Size = new System.Drawing.Size(90, 21);
			this.cbRedDots.TabIndex = 7;
			this.cbRedDots.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(43, 171);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(58, 13);
			this.label16.TabIndex = 4;
			this.label16.Text = "Scars (p7):";
			// 
			// cbScars
			// 
			this.cbScars.FormattingEnabled = true;
			this.cbScars.Location = new System.Drawing.Point(107, 168);
			this.cbScars.Name = "cbScars";
			this.cbScars.Size = new System.Drawing.Size(90, 21);
			this.cbScars.TabIndex = 5;
			this.cbScars.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(32, 144);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(69, 13);
			this.label17.TabIndex = 2;
			this.label17.Text = "Prisoner (p6):";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(12, 9);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(89, 13);
			this.label18.TabIndex = 8;
			this.label18.Text = "Background (p0):";
			// 
			// cbPrisoner
			// 
			this.cbPrisoner.FormattingEnabled = true;
			this.cbPrisoner.Location = new System.Drawing.Point(107, 141);
			this.cbPrisoner.Name = "cbPrisoner";
			this.cbPrisoner.Size = new System.Drawing.Size(90, 21);
			this.cbPrisoner.TabIndex = 8;
			this.cbPrisoner.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// cbBackground
			// 
			this.cbBackground.FormattingEnabled = true;
			this.cbBackground.Location = new System.Drawing.Point(107, 6);
			this.cbBackground.Name = "cbBackground";
			this.cbBackground.Size = new System.Drawing.Size(90, 21);
			this.cbBackground.TabIndex = 4;
			this.cbBackground.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// cbClothes
			// 
			this.cbClothes.FormattingEnabled = true;
			this.cbClothes.Location = new System.Drawing.Point(107, 60);
			this.cbClothes.Name = "cbClothes";
			this.cbClothes.Size = new System.Drawing.Size(90, 21);
			this.cbClothes.TabIndex = 7;
			this.cbClothes.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(35, 63);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(66, 13);
			this.label8.TabIndex = 7;
			this.label8.Text = "Clothes (p3):";
			// 
			// cbHeadgear
			// 
			this.cbHeadgear.FormattingEnabled = true;
			this.cbHeadgear.Location = new System.Drawing.Point(107, 114);
			this.cbHeadgear.Name = "cbHeadgear";
			this.cbHeadgear.Size = new System.Drawing.Size(90, 21);
			this.cbHeadgear.TabIndex = 6;
			this.cbHeadgear.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(23, 117);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(78, 13);
			this.label10.TabIndex = 3;
			this.label10.Text = "Headgear (p5):";
			// 
			// cbBeard
			// 
			this.cbBeard.FormattingEnabled = true;
			this.cbBeard.Location = new System.Drawing.Point(107, 87);
			this.cbBeard.Name = "cbBeard";
			this.cbBeard.Size = new System.Drawing.Size(90, 21);
			this.cbBeard.TabIndex = 5;
			this.cbBeard.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(42, 90);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(59, 13);
			this.label12.TabIndex = 4;
			this.label12.Text = "Beard (p4):";
			// 
			// cbHair
			// 
			this.cbHair.FormattingEnabled = true;
			this.cbHair.Location = new System.Drawing.Point(107, 33);
			this.cbHair.Name = "cbHair";
			this.cbHair.Size = new System.Drawing.Size(90, 21);
			this.cbHair.TabIndex = 4;
			this.cbHair.SelectedIndexChanged += new System.EventHandler(this.onChangeCharacteristic);
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(51, 36);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(50, 13);
			this.label14.TabIndex = 8;
			this.label14.Text = "Hair (p1):";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label21);
			this.groupBox2.Controls.Add(this.btnRandom);
			this.groupBox2.Controls.Add(this.btnImport);
			this.groupBox2.Controls.Add(this.cbPortraitTypes);
			this.groupBox2.Controls.Add(this.tabControl1);
			this.groupBox2.Location = new System.Drawing.Point(245, 10);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(224, 402);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Input";
			// 
			// label21
			// 
			this.label21.AutoSize = true;
			this.label21.Location = new System.Drawing.Point(8, 22);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(43, 13);
			this.label21.TabIndex = 24;
			this.label21.Text = "Portrait:";
			// 
			// btnRandom
			// 
			this.btnRandom.Location = new System.Drawing.Point(142, 375);
			this.btnRandom.Name = "btnRandom";
			this.btnRandom.Size = new System.Drawing.Size(75, 23);
			this.btnRandom.TabIndex = 11;
			this.btnRandom.Text = "Randomize";
			this.btnRandom.UseVisualStyleBackColor = true;
			this.btnRandom.Click += new System.EventHandler(this.onClickRandomize);
			// 
			// btnImport
			// 
			this.btnImport.Location = new System.Drawing.Point(6, 374);
			this.btnImport.Name = "btnImport";
			this.btnImport.Size = new System.Drawing.Size(75, 23);
			this.btnImport.TabIndex = 11;
			this.btnImport.Text = "Import";
			this.btnImport.UseVisualStyleBackColor = true;
			this.btnImport.Click += new System.EventHandler(this.onClickImport);
			// 
			// cbPortraitTypes
			// 
			this.cbPortraitTypes.FormattingEnabled = true;
			this.cbPortraitTypes.Location = new System.Drawing.Point(55, 19);
			this.cbPortraitTypes.Name = "cbPortraitTypes";
			this.cbPortraitTypes.Size = new System.Drawing.Size(164, 21);
			this.cbPortraitTypes.TabIndex = 2;
			this.cbPortraitTypes.SelectedIndexChanged += new System.EventHandler(this.onChangePortraitType);
			// 
			// tabContent
			// 
			this.tabContent.Controls.Add(this.tabDLCs);
			this.tabContent.Controls.Add(this.tabMods);
			this.tabContent.Location = new System.Drawing.Point(6, 17);
			this.tabContent.Name = "tabContent";
			this.tabContent.SelectedIndex = 0;
			this.tabContent.Size = new System.Drawing.Size(224, 356);
			this.tabContent.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
			this.tabContent.TabIndex = 4;
			// 
			// tabDLCs
			// 
			this.tabDLCs.AutoScroll = true;
			this.tabDLCs.Controls.Add(this.panelDLCs);
			this.tabDLCs.Location = new System.Drawing.Point(4, 22);
			this.tabDLCs.Name = "tabDLCs";
			this.tabDLCs.Padding = new System.Windows.Forms.Padding(3);
			this.tabDLCs.Size = new System.Drawing.Size(216, 330);
			this.tabDLCs.TabIndex = 1;
			this.tabDLCs.Text = "DLCs";
			this.tabDLCs.UseVisualStyleBackColor = true;
			// 
			// panelDLCs
			// 
			this.panelDLCs.AutoScroll = true;
			this.panelDLCs.Location = new System.Drawing.Point(4, 3);
			this.panelDLCs.Name = "panelDLCs";
			this.panelDLCs.Size = new System.Drawing.Size(207, 324);
			this.panelDLCs.TabIndex = 0;
			// 
			// tabMods
			// 
			this.tabMods.AutoScroll = true;
			this.tabMods.Controls.Add(this.panelMods);
			this.tabMods.Location = new System.Drawing.Point(4, 22);
			this.tabMods.Name = "tabMods";
			this.tabMods.Size = new System.Drawing.Size(216, 330);
			this.tabMods.TabIndex = 2;
			this.tabMods.Text = "Mods";
			this.tabMods.UseVisualStyleBackColor = true;
			// 
			// panelMods
			// 
			this.panelMods.AutoScroll = true;
			this.panelMods.Location = new System.Drawing.Point(5, 3);
			this.panelMods.Name = "panelMods";
			this.panelMods.Size = new System.Drawing.Size(208, 323);
			this.panelMods.TabIndex = 1;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.btnToogleAll);
			this.groupBox3.Controls.Add(this.btnReload);
			this.groupBox3.Controls.Add(this.tabContent);
			this.groupBox3.Location = new System.Drawing.Point(4, 10);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(237, 402);
			this.groupBox3.TabIndex = 5;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Content";
			// 
			// btnToogleAll
			// 
			this.btnToogleAll.Location = new System.Drawing.Point(6, 375);
			this.btnToogleAll.Name = "btnToogleAll";
			this.btnToogleAll.Size = new System.Drawing.Size(75, 23);
			this.btnToogleAll.TabIndex = 6;
			this.btnToogleAll.Text = "Toogle All";
			this.btnToogleAll.UseVisualStyleBackColor = true;
			this.btnToogleAll.Click += new System.EventHandler(this.onClickToogleAll);
			// 
			// btnReload
			// 
			this.btnReload.Location = new System.Drawing.Point(155, 374);
			this.btnReload.Name = "btnReload";
			this.btnReload.Size = new System.Drawing.Size(75, 23);
			this.btnReload.TabIndex = 5;
			this.btnReload.Text = "Reload";
			this.btnReload.UseVisualStyleBackColor = true;
			this.btnReload.Click += new System.EventHandler(this.onClickReload);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(666, 415);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.Text = "Portrait Builder";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbPortrait)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.tabContent.ResumeLayout(false);
			this.tabDLCs.ResumeLayout(false);
			this.tabMods.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.PictureBox pbPortrait;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.ComboBox cbMouth;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox cbEars;
		private System.Windows.Forms.ComboBox cbChin;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cbEyes;
		private System.Windows.Forms.ComboBox cbCheeks;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cbNose;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox cbNeck;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.ComboBox cbClothes;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.ComboBox cbHeadgear;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.ComboBox cbBeard;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.ComboBox cbHair;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.ComboBox cbRedDots;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.ComboBox cbBoils;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.ComboBox cbScars;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.ComboBox cbPrisoner;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.ComboBox cbBackground;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.TextBox tbDNA;
		private System.Windows.Forms.ComboBox cbEyeColour;
		private System.Windows.Forms.ComboBox cbHairColour;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.ComboBox cbRank;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnCopy;
		private System.Windows.Forms.Button btnImport;
		private System.Windows.Forms.Button btnRandom;
		private System.Windows.Forms.ComboBox cbPortraitTypes;
		private System.Windows.Forms.ComboBox cbBlinded;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.TabControl tabContent;
		private System.Windows.Forms.TabPage tabMods;
		private System.Windows.Forms.TabPage tabDLCs;
		private System.Windows.Forms.FlowLayoutPanel panelDLCs;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.FlowLayoutPanel panelMods;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button btnToogleAll;
		private System.Windows.Forms.Button btnReload;
		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.ComboBox cbPlayer;
	}
}
