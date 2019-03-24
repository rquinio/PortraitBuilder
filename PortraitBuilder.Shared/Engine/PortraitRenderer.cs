using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using log4net;
using PortraitBuilder.Model.Portrait;
using PortraitBuilder.Model.Content;

namespace PortraitBuilder.Engine {

	/// <summary>
	/// Handles the rendering of portraits
	/// </summary>
	public class PortraitRenderer {

		private static readonly ILog logger = LogManager.GetLogger(typeof(PortraitRenderer));

		private static String[] governmentSpriteSuffix = new String[] { "", "_iqta", "_theocracy", "_republic", "_merchantrepublic", "_tribal", "_nomadic", "_theocraticfeudal", "_chineseimperial", "_confucian" };

		/// <summary>
		/// Draws a character portrait.
		/// </summary>
		/// <param name="portraitType">PortaitType to use for drawing.</param>
		/// <param name="portrait">Portrait input to draw.</param>
		/// <param name="activeContents">Content to load sprites from</param>
		/// <returns>Frameless portrait drawn with the given parameters.</returns>
		public Bitmap DrawPortrait(Portrait portrait, List<Content> activeContents, Dictionary<string, Sprite> sprites) {
			logger.Info(string.Format("Drawing Portrait {0}", portrait));

			Bitmap portraitImage = new Bitmap(176, 176);
			Graphics g = Graphics.FromImage(portraitImage);

			foreach (Layer layer in portrait.GetPortraitType().Layers) {
				DrawLayer(layer, g, portrait, activeContents, sprites);
			}

			DrawBorder(portrait, g, activeContents, sprites);

			g.Dispose();
			return portraitImage;
		}

		private void DrawLayer(Layer layer, Graphics g, Portrait portrait, List<Content> activeContents, Dictionary<string, Sprite> sprites) {
			logger.Debug("Drawing Layer : " + layer);

			string spriteName = GetOverriddenSpriteName(portrait, layer);

			// Backup for merchants, which are part of "The Republic" DLC !
			if (!sprites.ContainsKey(spriteName)) {
				spriteName = layer.Name;
			}

			try {
				if (sprites.ContainsKey(spriteName)) {
					Sprite sprite = sprites[spriteName];

					//Check if loaded; if not, then load
					if (!sprite.IsLoaded) {
						LoadSprite(sprite, activeContents);
					}

					//Get DNA/Properties letter, then the index of the tile to draw
					int tileIndex = GetTileIndex(portrait, sprite.FrameCount, layer);

					DrawTile(portrait, g, sprite, layer, tileIndex);
				}
				else {
					throw new FileNotFoundException("Sprite not found:" + spriteName);
				}
			}
			catch (Exception e) {
				logger.Error("Could not render layer " + layer, e);
			}
		}

		/// <summary>
		/// Override sprite for religious/merchant
		/// 
		/// This is quite messy - not sure what hardcoded vanilla logic exactly is - could be based on culture index ?!
		/// </summary>
		/// <param name="portrait"></param>
		/// <param name="layer"></param>
		/// <returns></returns>
		private string GetOverriddenSpriteName(Portrait portrait, Layer layer) {
			string spriteName = layer.Name;
			if ((portrait.IsReligious() || portrait.IsMerchant()) && (layer.Characteristic == Characteristic.CLOTHES || layer.Characteristic == Characteristic.HEADGEAR)) {
				string sex = portrait.GetSex() == Portrait.Sex.MALE ? "male" : "female";
				string layerSuffix = spriteName.Contains("behind") ? "_behind" : ""; // Handles clothes_infront and headgear_mid
				string government = portrait.IsReligious() ? "religious" : "merchant";
				string layerType = layer.Characteristic == Characteristic.CLOTHES ? "clothes" : "headgear";
				spriteName = string.Format("GFX_{0}_{1}_{2}{3}", government, sex, layerType, layerSuffix);
			}
			return spriteName;
		}

		private void DrawBorder(Portrait portrait, Graphics g, List<Content> activeContents,  Dictionary<string, Sprite> sprites) {
			logger.Debug("Drawing border.");
			try {
				string governmentSpriteName = "GFX_charframe_150" + governmentSpriteSuffix[portrait.GetGovernment()];
				if (sprites.ContainsKey(governmentSpriteName)) {
					Sprite sprite = sprites[governmentSpriteName];

					//Check if loaded; if not, then load
					if (!sprite.IsLoaded) {
						LoadSprite(sprite, activeContents);
					}
					g.DrawImage(sprite.Tiles[portrait.GetRank()], 0, 0);
				}
			}
			catch (Exception e) {
				logger.Error("Could not render borders ", e);
			}
		}

		private int GetTileIndex(Portrait portrait, int frameCount, Layer layer) {
			char letter = portrait.GetLetter(layer.Characteristic);
			int tileIndex = Portrait.GetIndex(letter, frameCount);
			logger.Debug(string.Format("Layer letter: {0}, Tile Index: {1}", letter, tileIndex));
			return tileIndex;
		}

		private void LoadSprite(Sprite sprite, List<Content> activeContents) {
            // Paths in vanilla files are Windows-style
			string filePath = sprite.TextureFilePath.Replace('\\', Path.DirectorySeparatorChar);

			// Also try alternative extension (.tga <=> .dds)
			string extension = filePath.Substring(filePath.Length - 4);
			string alternative = extension == ".dds" ? ".tga" : ".dds";
			string alternativeFilePath = filePath.Replace(extension, alternative);

			string path = null;

			// Loop on reverse order - last occurence wins if asset is overriden !
			for (int i = activeContents.Count - 1; i >= 0; i--) {
				Content content = activeContents[i];
				path = content.AbsolutePath + Path.DirectorySeparatorChar + filePath;

				if (!File.Exists(path)) {
					path = content.AbsolutePath + Path.DirectorySeparatorChar + alternativeFilePath;
				}
				if (File.Exists(path)) {
					break;
				}
			}

			if (path != null) {
				logger.Debug("Loading sprite from: " + path);
				sprite.Load(path);
			} else {
				throw new FileNotFoundException(string.Format("Unable to find file: {0} under active content {1}", filePath, activeContents));
			}
		}

		private void DrawTile(Portrait portrait, Graphics g, Sprite sprite, Layer layer, int tileIndex) {
			Bitmap tile;
			if (layer.IsHair) {
				List<Hair> hairCoulours = portrait.GetPortraitType().HairColours;
				int hairIndex = Portrait.GetIndex(portrait.GetLetter(Characteristic.HAIR_COLOR), hairCoulours.Count);
				tile = DrawHair(sprite.Tiles[tileIndex], hairCoulours[hairIndex]);

			} else if (layer.IsEye) {
				List<Colour> eyeCoulours = portrait.GetPortraitType().EyeColours;
				int eyeIndex = Portrait.GetIndex(portrait.GetLetter(Characteristic.EYE_COLOR), eyeCoulours.Count);
				tile = DrawEye(sprite.Tiles[tileIndex], eyeCoulours[eyeIndex]);

			} else {
				tile = sprite.Tiles[tileIndex];
			}

			g.DrawImage(tile, 12 + layer.Offset.X, 12 + 152 - tile.Size.Height - layer.Offset.Y);
		}

		/// <summary>
		/// Based on gfx\FX\portrait.lua EyePixelShader
		/// </summary>
		private Bitmap DrawEye(Bitmap source, Colour eyeColour) {
			Bitmap output = new Bitmap(source.Width, source.Height);
			Colour colour1 = new Colour();
			Colour colour2 = new Colour();

			BitmapData bdata = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, source.PixelFormat);
			BitmapData odata = output.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, output.PixelFormat);
			int pixelSize = 4;

			unsafe {
				for (int y = 0; y < source.Height; y++) {
					byte* brow = (byte*)bdata.Scan0 + (y * bdata.Stride);
					byte* orow = (byte*)odata.Scan0 + (y * odata.Stride);

					for (int x = 0; x < source.Width; x++) {
						colour1.Red = brow[x * pixelSize + 2];
						colour1.Alpha = brow[x * pixelSize + 3];

						if (colour1.Alpha > 0) {
							colour2.Red = (byte)(255 * ((eyeColour.Red / 255f) * (colour1.Red / 255f)));
							colour2.Green = (byte)(255 * ((eyeColour.Green / 255f) * (colour1.Red / 255f)));
							colour2.Blue = (byte)(255 * ((eyeColour.Blue / 255f) * (colour1.Red / 255f)));
							colour2.Alpha = colour1.Alpha;

							orow[x * pixelSize] = colour2.Blue;
							orow[x * pixelSize + 1] = colour2.Green;
							orow[x * pixelSize + 2] = colour2.Red;
							orow[x * pixelSize + 3] = colour2.Alpha;
						}
					}
				}
			}

			source.UnlockBits(bdata);
			output.UnlockBits(odata);

			return output;
		}

		/// <summary>
		/// Based on gfx\FX\portrait.lua HairPixelShader
		/// </summary>
		private Bitmap DrawHair(Bitmap source, Hair hair) {
			Bitmap output = new Bitmap(source.Width, source.Height);
			Colour colour1 = new Colour();

			BitmapData bdata = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, source.PixelFormat);
			BitmapData odata = output.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, output.PixelFormat);
			int pixelSize = 4;

            //FIXME: spanify
			unsafe {
				for (int y = 0; y < source.Height; y++) {
					byte* brow = (byte*)bdata.Scan0 + (y * bdata.Stride);
					byte* orow = (byte*)odata.Scan0 + (y * odata.Stride);

					for (int x = 0; x < source.Width; x++) {
						colour1.Green = brow[x * pixelSize + 1];
						colour1.Alpha = brow[x * pixelSize + 3];

						if (colour1.Alpha > 0) {
							Colour colour2 = Colour.Lerp(hair.Dark, hair.Base, Colour.Clamp(colour1.Green * 2));
							colour2 = Colour.Lerp(colour2, hair.Highlight, Colour.Clamp((colour1.Green - 128f) * 2));
							colour2.Alpha = colour1.Alpha;

							orow[x * pixelSize] = colour2.Blue;
							orow[x * pixelSize + 1] = colour2.Green;
							orow[x * pixelSize + 2] = colour2.Red;
							orow[x * pixelSize + 3] = colour2.Alpha;
						}
					}
				}
			}

			source.UnlockBits(bdata);
			output.UnlockBits(odata);

			return output;
		}
	}
}
