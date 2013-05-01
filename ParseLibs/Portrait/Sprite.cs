using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Parsers.Portrait
{
	public class Sprite
	{
		public string Name;
		public string TextureFilePath;

		public int FrameCount;

		public bool NoRefCount;
		public bool IsLoaded;

		public List<Bitmap> Tiles = new List<Bitmap>();

		/// <summary>
		/// Used for storing data specific to the program.
		/// </summary>
		public Dictionary<string, object> CustomFlags = new Dictionary<string, object>();

		/// <summary>
		/// The file that the data was loaded from.
		/// </summary>
		public string Filename;

		/// <summary>
		/// Loads the tiles in the sprite. Sets IsLoaded to true.
		/// </summary>
		/// <param name="ckDir">Directory to load image from.</param>
		public void Load( string ckDir )
		{
			Bitmap texture, tile;
			Graphics g;
			Size s;
			Rectangle drawArea;

			if( Tiles.Count > 0 )
			{
				foreach( Bitmap b in Tiles )
					b.Dispose();
				Tiles = new List<Bitmap>();
			}

			if( File.Exists( ckDir + "/" + TextureFilePath ) )
				texture = DevIL.DevIL.LoadBitmap( ckDir + "/" + TextureFilePath );
			else
				throw new FileLoadException( "Unable to find texture file.", ckDir + TextureFilePath );

			s = new Size( texture.Width / FrameCount, texture.Height );

			for( int i = 0; i < FrameCount; i++ )
			{
				tile = new Bitmap( s.Width, s.Height );
				g = Graphics.FromImage( tile );
				drawArea = new Rectangle( i * s.Width, 0, s.Width, s.Height );
				g.DrawImage( texture, 0, 0, drawArea, GraphicsUnit.Pixel );
				Tiles.Add( tile );
				g.Dispose();
			}

			texture.Dispose();

			IsLoaded = true;
		}

		public override string ToString()
		{
			return string.Format( "Name: {0}, FrameCount: {1}, IsLoaded: {2}", Name, FrameCount, IsLoaded );
		}
	}
}