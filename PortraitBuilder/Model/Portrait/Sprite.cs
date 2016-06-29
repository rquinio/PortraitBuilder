using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CSharpImageLibrary.General;

namespace PortraitBuilder.Model.Portrait {
	public class Sprite {
		public string Name;
		public string TextureFilePath;

		public int FrameCount;

		public bool NoRefCount;
		public bool IsLoaded;

		public List<Bitmap> Tiles = new List<Bitmap>();

		/// <summary>
		/// The file that the data was loaded from.
		/// </summary>
		public string Filename;

		/// <summary>
		/// Loads the tiles in the sprite. Sets IsLoaded to true.
		/// </summary>
		/// <param name="filePath">Path to the image.</param>
		public void Load(string filePath) {
			Bitmap texture;

			Unload();

			if (File.Exists(filePath)) {
				texture = new ImageEngineImage(filePath).GetGDIBitmap(false, false);
			} else {
				throw new FileLoadException("Unable to find texture file", filePath);
			}

			if(texture == null) {
				throw new FileLoadException("Texture file is empty.", filePath);
			}

			Size size = new Size(texture.Width / FrameCount, texture.Height);

			for (int indexFrame = 0; indexFrame < FrameCount; indexFrame++) {
				Bitmap tile = new Bitmap(size.Width, size.Height);
				Graphics g = Graphics.FromImage(tile);
				Rectangle drawArea = new Rectangle(indexFrame * size.Width, 0, size.Width, size.Height);
				g.DrawImage(texture, 0, 0, drawArea, GraphicsUnit.Pixel);
				Tiles.Add(tile);
				g.Dispose();
			}

			texture.Dispose();

			IsLoaded = true;
		}

		public void Unload() {
			if (Tiles.Count > 0) {
				foreach (Bitmap tile in Tiles) {
					tile.Dispose();
				}
				IsLoaded = false;
				Tiles = new List<Bitmap>();
			}
		}

		public override string ToString() {
			return string.Format("Name: {0}, Texture: {1}, FrameCount: {2}, IsLoaded: {3}", Name, TextureFilePath, FrameCount, IsLoaded);
		}
	}
}