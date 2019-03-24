using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

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
                texture = LoadDDS(filePath);
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

        private static Bitmap LoadDDS(string filepath)
        {
            var image = Pfim.Pfim.FromFile(filepath);

            PixelFormat format;
            switch (image.Format)
            {
                case Pfim.ImageFormat.Rgb24:
                    format = PixelFormat.Format24bppRgb;
                    break;

                case Pfim.ImageFormat.Rgba32:
                    format = PixelFormat.Format32bppArgb;
                    break;

                default:
                    throw new Exception("Format not recognized");
            }

            unsafe
            {
                fixed (byte* p = image.Data)
                {
                    return new Bitmap(image.Width, image.Height, image.Stride, format, (IntPtr)p);
                }
            }
        }

        public override string ToString() {
			return string.Format("Name: {0}, Texture: {1}, FrameCount: {2}, IsLoaded: {3}", Name, TextureFilePath, FrameCount, IsLoaded);
		}
	}
}