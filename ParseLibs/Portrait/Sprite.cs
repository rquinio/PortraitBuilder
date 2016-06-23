using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Parsers.Portrait {
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
		/// <param name="dir">Directory to load image from.</param>
		public void Load(string dir) {
			Bitmap texture;

			if (Tiles.Count > 0) {
				foreach (Bitmap tile in Tiles)
					tile.Dispose();
				Tiles = new List<Bitmap>();
			}

			if (File.Exists(dir + "/" + TextureFilePath))
				texture = DevIL.DevIL.LoadBitmap(dir + "/" + TextureFilePath);
			else
				throw new FileLoadException("Unable to find texture file.", dir + TextureFilePath);

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

		public override string ToString() {
			return string.Format("Name: {0}, Texture: {1}, FrameCount: {2}, NoRefCount: {3}, IsLoaded: {4}", Name, TextureFilePath, FrameCount, NoRefCount, IsLoaded);
		}
	}
}