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
                //Engine.PortraitRenderer.debug(texture);

                var src = new SKRectI(0, 0, texture.Width / FrameCount, texture.Height);
                var dst = new SKRectI(0, 0, src.Width, src.Height);

                for (int i = 0; i < FrameCount; i++)
                {
                    var tile = new SKBitmap(src.Width, src.Height);
                    using (var canvas = new SKCanvas(tile))
                    {
                        //must set transparent bg for unpremul -> premul
                        canvas.Clear(SKColors.Transparent);
                        canvas.DrawBitmap(texture, src, dst);
                    }
                    //Engine.PortraitRenderer.debug(tile);
                    Tiles.Add(tile);

                    src.Offset(src.Width, 0);
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
            Debug.Assert(image.Compressed == false);

            var info = new SKImageInfo(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            var bmp = new SKBitmap(info);
            unsafe
            {
                fixed (byte* pData = image.Data)
                {
                    bmp.InstallPixels(info, (IntPtr)pData, image.Stride);
                }
                return bmp;
            }
        }

        public override string ToString()
            => $"Name: {Name}, Texture: {TextureFilePath}, FrameCount: {FrameCount}, IsLoaded: {IsLoaded}";
    }
}