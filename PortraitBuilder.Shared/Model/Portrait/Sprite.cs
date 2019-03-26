using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PortraitBuilder.Model.Portrait
{
    public class Sprite
    {
        public string Name;
        public string TextureFilePath;

        public int FrameCount;

        public bool NoRefCount;
        public bool IsLoaded;

        public List<SKBitmap> Tiles { get; private set; } = new List<SKBitmap>();

        /// <summary>
        /// The file that the data was loaded from.
        /// </summary>
        public string Filename { get; }

        public Sprite(string filename)
        {
            this.Filename = filename;
        }

        /// <summary>
        /// Loads the tiles in the sprite. Sets IsLoaded to true.
        /// </summary>
        /// <param name="filePath">Path to the image.</param>
        public void Load(string filePath)
        {
            Unload();

            if (!File.Exists(filePath))
            {
                throw new FileLoadException("Unable to find texture file", filePath);
            }

            using (var texture = LoadDDS(filePath))
            {
                Size size = new Size(texture.Width / FrameCount, texture.Height);
                for (int indexFrame = 0; indexFrame < FrameCount; indexFrame++)
                {
                    SKBitmap tile = new Bitmap(size.Width, size.Height);
                    using (Graphics g = Graphics.FromImage(tile))
                    {
                        Rectangle drawArea = new Rectangle(indexFrame * size.Width, 0, size.Width, size.Height);
                        g.DrawImage(texture, 0, 0, drawArea, GraphicsUnit.Pixel);
                    }
                    Tiles.Add(tile);
                }
            }

            IsLoaded = true;
        }

        public void Unload()
        {
            if (!Tiles.Any())
                return;

            foreach (var tile in Tiles)
            {
                tile.Dispose();
            }
            IsLoaded = false;
            Tiles = new List<SKBitmap>();
        }

        private static SKBitmap LoadDDS(string filepath)
        {
            var image = Pfim.Pfim.FromFile(filepath);
            Debug.Assert(image.Format == Pfim.ImageFormat.Rgba32);

            GCHandle? handle = null;
            try
            {
                handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);

                var bmp = new SKBitmap(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
                bmp.SetPixels(handle.Value.AddrOfPinnedObject());

                return bmp;
            }
            finally
            {
                handle?.Free();
            }
        }

        public override string ToString()
            => $"Name: {Name}, Texture: {TextureFilePath}, FrameCount: {FrameCount}, IsLoaded: {IsLoaded}";
    }
}