using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Hime.Redist.Parsers;
using log4net;
using PortraitBuilder.Model.Portrait;

namespace PortraitBuilder.Parser {

	/// <summary>
	/// Handles the parsing of portraits *.gfx files.
	/// </summary>
	public class PortraitReader {

		private static readonly ILog logger = LogManager.GetLogger(typeof(PortraitReader).Name);

		/// <summary>
		/// Stateless portrait_offsets.txt file scanner
		/// </summary>
		private PortraitOffsetReader portraitOffsetReader = new PortraitOffsetReader();

		/// <summary>
		/// Parse Portrait data.
		/// 
		/// Parsing errors are catched at layer, propertyType or file level, so the PortraitData may be partial or even empty.
		/// </summary>
		/// <param name="dir">The content root directory to parse from</param>
		/// <returns></returns>
		public PortraitData Parse(string dir) {
			PortraitData data = new PortraitData();
			try {
				List<string> fileNames = new List<string>();

				if (Directory.Exists(dir + @"\interface\")) {
					fileNames.AddRange(Directory.GetFiles(dir + @"\interface\", "*.gfx"));
				}
				// interface/portraits seems to be loaded after interface/, and override (cf byzantinegfx)
				if (Directory.Exists(dir + @"\interface\portraits\")) {
					fileNames.AddRange(Directory.GetFiles(dir + @"\interface\portraits\", "*.gfx"));
				}

				foreach (string fileName in fileNames) {
					Parse(fileName, data);
				}

				if (Directory.Exists(dir + @"\interface\portrait_offsets\")) {
					string[] offsetFileNames = Directory.GetFiles(dir + @"\interface\portrait_offsets\", "*.txt");
					foreach (string offsetFileName in offsetFileNames) {
						Dictionary<string, Point> offsets = portraitOffsetReader.Parse(offsetFileName);
						data.Offsets = data.Offsets.Concat(offsets).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
					}
				}
			} catch (Exception e) {
				logger.Error("Failed to parse portrait data in " + dir, e);
			}
			
			return data;
		}

		/// <summary>
		/// Parses a given portrait.gfx file.
		/// </summary>
		/// <param name="filename">Path of the file to parse.</param>
		private void Parse(string filename, PortraitData data) {
			if (!File.Exists(filename)) {
				logger.Error(string.Format("File not found: {0}", filename));
				return;
			}

			StreamReader stream = new StreamReader(filename, Encoding.GetEncoding(1252));
			string fileContent = stream.ReadToEnd();
			stream.Dispose();

			//Check the file isn't empty
			string[] lines = fileContent.Split(fileContent.Contains('\r') ? '\r' : '\n');
			bool isEmpty = true;
			for (int i = 0; i < lines.Length; i++) {
				if (!lines[i].Trim().StartsWith("#") && !String.IsNullOrEmpty(lines[i].Trim())) {
					isEmpty = false;
					break;
				}
			}

			if (isEmpty) {
				logger.Error("File is empty: " + filename);
				return;
			}

			//Parse the file
			PortraitReaderLexer lexer = new PortraitReaderLexer(fileContent);
			PortraitReaderParser parser = new PortraitReaderParser(lexer);

			SyntaxTreeNode root = parser.Analyse();

			if (root == null) {
				logger.Error(String.Format("Lexical error in file {0}, line {1}", (new FileInfo(filename).Name), lexer.CurrentLine));
				return;
			}

			ParseTree(root, filename, data);
		}

		private void ParseTree(SyntaxTreeNode root, string filename, PortraitData data) {
			foreach (SyntaxTreeNode child in root.Children) {
				ParsePortraits(child, filename, data);
			}
		}

		private void ParsePortraits(SyntaxTreeNode node, string filename, PortraitData data) {
			IEnumerable<SyntaxTreeNode> children = node.Children.Where(child => child.Symbol.Name == "groupOption");
			SymbolTokenText id;

			foreach (SyntaxTreeNode child in children) {
				id = child.Children[0].Symbol as SymbolTokenText;

				if (id.ValueText == "spriteType") {
					try {
						Sprite sprite = ParseSpriteType(child, filename);
						if (data.Sprites.ContainsKey(sprite.Name)) {
							logger.Debug("Sprite already exists. Replacing.");
							data.Sprites.Remove(sprite.Name);
						}
						data.Sprites.Add(sprite.Name, sprite);
					} catch (Exception e) {
						logger.Error(string.Format("Could not parse spriteType in file {0}", filename), e);
					}
				} else if (id.ValueText == "portraitType") {
					try {
						PortraitType portraitType = ParsePortraitType(child, filename);
						if (data.PortraitTypes.ContainsKey(portraitType.Name)) {
							logger.Debug("Portrait type " + portraitType.Name + "exists. Replacing.");
							data.PortraitTypes.Remove(portraitType.Name);
						}
						data.PortraitTypes.Add(portraitType.Name, portraitType);

					} catch (Exception e) {
						logger.Error(string.Format("Could not parse portraitType in file {0}", filename), e);
					}
				}
			}
		}

		private PortraitType ParsePortraitType(SyntaxTreeNode node, string filename) {
			PortraitType portraitType = new PortraitType();
			portraitType.Filename = filename;

			List<SyntaxTreeNode> children = node.Children.Where(child => child.Symbol.Name == "Option").ToList();
			SymbolTokenText id, value;
			SyntaxTreeNode token;
			foreach (SyntaxTreeNode child in children) {
				token = child.Children[0];

				if (token.Children.Count > 1 == false)
					continue;

				id = token.Children[0].Symbol as SymbolTokenText;
				value = token.Children[1].Symbol as SymbolTokenText;

				switch (token.Symbol.Name) {
				case "stringOption":
					if (id.ValueText == "name")
						portraitType.Name = value.ValueText.Replace("\"", "");
					if (id.ValueText == "effectFile")
						portraitType.EffectFile = value.ValueText.Replace("\"", "").Replace(@"\\", @"\");
					break;
				case "numberOption":
					if (id.ValueText == "hair_color_index")
						portraitType.HairColourIndex = Int32.Parse(value.ValueText);
					if (id.ValueText == "eye_color_index")
						portraitType.EyeColourIndex = Int32.Parse(value.ValueText);
					break;
				}
			}

			logger.Debug("Type parsed: ");
			logger.Debug(" --ID: " + portraitType.Name);
			logger.Debug(" --Hair Colour Index: " + portraitType.HairColourIndex);
			logger.Debug(" --Eye Colour Index: " + portraitType.EyeColourIndex);

			portraitType.Layers.AddRange(ParseLayers(node.Children.Single(c => c.Symbol.Name == "layerGroup"), filename));

			children = node.Children.Where(c => c.Symbol.Name == "cultureGroup").ToList();
			if (children.Count > 0) {
				foreach (SyntaxTreeNode child in children[0].Children)
					portraitType.Culture.Add(Int32.Parse(((SymbolTokenText)child.Symbol).ValueText));
			}

			children = node.Children.Where(c => c.Symbol.Name == "groupOption").ToList();

			foreach (SyntaxTreeNode child in children) {
				id = child.Children[0].Symbol as SymbolTokenText;

				if (id.ValueText == "hair_color") {
					portraitType.HairColours.AddRange(ParseHairColours(child));
				} else if (id.ValueText == "eye_color") {
					portraitType.EyeColours.AddRange(ParseEyeColours(child));
				}
			}

			return portraitType;
		}

		private List<Colour> ParseEyeColours(SyntaxTreeNode node) {
			List<Colour> colours = new List<Colour>();
			IEnumerable<SyntaxTreeNode> children = node.Children.Where(child => child.Symbol.Name == "colourGroup");

			foreach (SyntaxTreeNode child in children) {
				colours.Add(ParseColour(child));
			}
			return colours;
		}

		private List<Hair> ParseHairColours(SyntaxTreeNode node) {
			List<Hair> hairs = new List<Hair>();
			List<SyntaxTreeNode> children = node.Children.Where(child => child.Symbol.Name == "colourGroup").ToList();

			for (int i = 0; i < children.Count; i += 3) {
				logger.Debug(" --Parsing Hair colours");

				Hair hair = new Hair();

				hair.Dark = ParseColour(children[i]);
				logger.Debug("   --Dark: " + hair.Dark);

				hair.Base = ParseColour(children[i + 1]);
				logger.Debug("   --Base: " + hair.Base);

				hair.Highlight = ParseColour(children[i + 2]);
				logger.Debug("   --Highlight: " + hair.Highlight);

				hairs.Add(hair);
			}
			return hairs;
		}

		private Colour ParseColour(SyntaxTreeNode child) {
			Colour colour = new Colour();

			SymbolTokenText value = child.Children[0].Symbol as SymbolTokenText;
			colour.Red = byte.Parse(value.ValueText);
			value = child.Children[1].Symbol as SymbolTokenText;
			colour.Green = byte.Parse(value.ValueText);
			value = child.Children[2].Symbol as SymbolTokenText;
			colour.Blue = byte.Parse(value.ValueText);

			logger.Debug(" --Colour Parsed: " + colour);
			return colour;
		}

		private List<Layer> ParseLayers(SyntaxTreeNode node, string filename) {
			List<Layer> layers = new List<Layer>();
			foreach (SyntaxTreeNode child in node.Children) {
				layers.Add(ParseLayer(child, filename));
			}
			return layers;
		}

		private Layer ParseLayer(SyntaxTreeNode node, string filename) {
			string[] layerParts = ((SymbolTokenText)node.Symbol).ValueText.Replace("\"", "").Split(':');

			Layer layer = new Layer();
			layer.Filename = filename;

			layer.Name = layerParts[0];

			int index = int.Parse(layerParts[1].Substring(1));
			if (layerParts[1][0] == 'd') {
				layer.Characteristic = Characteristic.DNA[index];
			} else if (layerParts[1][0] == 'p') {
				layer.Characteristic = Characteristic.PROPERTIES[index];
			} else {
				logger.Error(string.Format("Unkown type {0}, for layer {1} in file {2}", layerParts[1], layer, filename));
			}

			for (int i = 2; i < layerParts.Length; i++) {
				if (layerParts[i] == "h" || layerParts[2] == "x") {
					layer.IsHair = true;
				} else if (layerParts[i] == "e") {
					layer.IsEye = true;
				} else if (layerParts[i] == "y") {
					layer.DontRefreshIfValid = true;
				} else if (layerParts[i].StartsWith("o")) {
					string[] offsets = layerParts[i].Substring(1).Split('x');
					layer.Offset = new Point(int.Parse(offsets[0]), int.Parse(offsets[1]));
				}
			}

			logger.Debug(" --Layer Parsed: " + layer);

			return layer;
		}

		private Sprite ParseSpriteType(SyntaxTreeNode node, string filename) {
			Sprite sprite = new Sprite();
			sprite.Filename = filename;

			IEnumerable<SyntaxTreeNode> children = node.Children.Where(child => child.Symbol.Name == "Option");
			SymbolTokenText id, value;
			SyntaxTreeNode token;
			foreach (SyntaxTreeNode child in children) {
				token = child.Children[0];

				if (token.Children.Count > 1 == false)
					continue;

				id = token.Children[0].Symbol as SymbolTokenText;
				value = token.Children[1].Symbol as SymbolTokenText;

				switch (token.Symbol.Name) {
				case "stringOption":
					if (id.ValueText == "name")
						sprite.Name = value.ValueText.Replace("\"", "");
					if (id.ValueText == "texturefile")
						sprite.TextureFilePath = value.ValueText.Replace("\"", "").Replace(@"\\", @"\");
					break;
				case "boolOption":
					if (id.ValueText == "norefcount")
						sprite.NoRefCount = value.ValueText == "yes";
					break;
				case "numberOption":
					if (id.ValueText == "noOfFrames")
						sprite.FrameCount = Int32.Parse(value.ValueText);
					break;
				}
			}
			logger.Debug("Sprite Parsed: " + sprite);

			return sprite;
		}
	}
}
