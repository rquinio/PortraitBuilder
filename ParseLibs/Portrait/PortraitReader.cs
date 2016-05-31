using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Hime.Redist.Parsers;
using Parsers.Mod;

namespace Parsers.Portrait
{
	public class PortraitReader
	{
		/// <summary>
		/// Dictionary of loaded Sprites.
		/// Key is the name of the sprite. E.g. GFX_character_background
		/// </summary>
		public Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
		/// <summary>
		/// Dictionary of loaded Portrait Types.
		/// Key is the name of the Portrait Type. E.g. PORTRAIT_westerngfx_male
		/// </summary>
		public Dictionary<string, PortraitType> PortraitTypes = new Dictionary<string, PortraitType>();

		/// <summary>
		/// List of errors encountered during parsing.
		/// </summary>
		public List<string> Errors = new List<string>();
		/// <summary>
		/// List of errors encountered during rendering. Clears every time DrawPortrait is called.
		/// </summary>
		public List<string> DrawErrors;

		public List<char> Letters = new List<char> { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
		private Random m_rand = new Random();

		private bool m_logging = false;
		private StreamWriter m_log;

		/// <summary>
		/// Unloads all loaded portrait data.
		/// </summary>
		public void Dispose()
		{
			foreach( KeyValuePair<string, Sprite> pair in Sprites )
				if( pair.Value.IsLoaded )
					foreach( Bitmap b in pair.Value.Tiles )
						b.Dispose();

			Sprites.Clear();
			PortraitTypes.Clear();
		}

		public void SetLogging( StreamWriter log )
		{
			m_log = log;
			m_logging = true;
		}

		private void Log( string text )
		{
            System.Diagnostics.Debug.WriteLine(text);
			if( m_logging )
			{
				m_log.WriteLine( "PRC > " + text );
				m_log.Flush();
			}
		}

		/// <summary>
		/// Parses a given portrait.gfx file. Any errors encountered are stored in the Errors list.
		/// </summary>
		/// <param name="filename">Path of the file to parse.</param>
		public void Parse( string filename )
		{
			Log( "Checking file exists: " + filename );
			if( !File.Exists( filename ) )
			{
				Errors.Add( string.Format( "File not found: {0}", filename ) );
				Log( "File not found." );
				return;
			}

			StreamReader relfile = new StreamReader( filename, Encoding.GetEncoding( 1252 ) );
			string relcontents = relfile.ReadToEnd();
			relfile.Dispose();

			//Check the file isn't empty
			string[] lines = relcontents.Split( relcontents.Contains( '\r' ) ? '\r' : '\n' );
			bool isEmpty = true;
			for( int i = 0; i < lines.Length; i++ )
			{
				if( !lines[i].Trim().StartsWith( "#" ) && !String.IsNullOrEmpty( lines[i].Trim() ) )
				{
					isEmpty = false;
					break;
				}
			}

			if( isEmpty )
			{
				Log( "File is empty: " + filename );
				return;
			}

			//Parse the file
            PortraitReaderLexer lexer = new PortraitReaderLexer(relcontents);
            PortraitReaderParser parser = new PortraitReaderParser(lexer);

			SyntaxTreeNode root = parser.Analyse();

			if( root == null )
			{
				Errors.Add( String.Format( "Lexical error in file {0}, line {1}", ( new FileInfo( filename ).Name ), lexer.CurrentLine ) );
				return;
			}

			ParseTree( root, filename );
		}

		private void ParseTree( SyntaxTreeNode root, string filename )
		{
			foreach( SyntaxTreeNode child in root.Children )
			{
				ParsePortraits( child, filename );
			}
		}

		private void ParsePortraits( SyntaxTreeNode node, string filename )
		{
			IEnumerable<SyntaxTreeNode> children = node.Children.Where( child => child.Symbol.Name == "groupOption" );
			SymbolTokenText id;

			foreach( SyntaxTreeNode child in children )
			{
				id = child.Children[0].Symbol as SymbolTokenText;

				if( id.ValueText == "spriteType" )
				{
					ParseSpriteType( child, filename );
				} else if( id.ValueText == "portraitType" )
				{
					ParsePortraitType( child, filename );
				}
			}
		}

		private void ParsePortraitType( SyntaxTreeNode node, string filename )
		{
			PortraitType p = new PortraitType();
			p.Filename = filename;

			List<SyntaxTreeNode> children = node.Children.Where( child => child.Symbol.Name == "Option" ).ToList();
			SymbolTokenText id, value;
			SyntaxTreeNode token;
			foreach( SyntaxTreeNode child in children )
			{
				token = child.Children[0];

				if( token.Children.Count > 1 == false )
					continue;

				id = token.Children[0].Symbol as SymbolTokenText;
				value = token.Children[1].Symbol as SymbolTokenText;

				switch( token.Symbol.Name )
				{
					case "stringOption":
						if( id.ValueText == "name" )
							p.Name = value.ValueText.Replace( "\"", "" );
						if( id.ValueText == "effectFile" )
							p.EffectFile = value.ValueText.Replace( "\"", "" ).Replace( @"\\", @"\" );
						break;
					case "numberOption":
						if( id.ValueText == "hair_color_index" )
							p.HairColourIndex = Int32.Parse( value.ValueText );
						if( id.ValueText == "eye_color_index" )
							p.EyeColourIndex = Int32.Parse( value.ValueText );
						break;
				}
			}

			Log( "Type parsed: " );
			Log( " --ID: " + p.Name );
			Log( " --Hair Colour Index: " + p.HairColourIndex );
			Log( " --Eye Colour Index: " + p.EyeColourIndex );

			ParseLayers( p, node.Children.Single( c => c.Symbol.Name == "layerGroup" ), filename );

			children = node.Children.Where( c => c.Symbol.Name == "cultureGroup" ).ToList();
			if( children.Count > 0 )
			{
				foreach( SyntaxTreeNode child in children[0].Children )
					p.Culture.Add( Int32.Parse( ( (SymbolTokenText)child.Symbol ).ValueText ) );
			}

			children = node.Children.Where( c => c.Symbol.Name == "groupOption" ).ToList();

			foreach( SyntaxTreeNode child in children )
			{
				id = child.Children[0].Symbol as SymbolTokenText;

				if( id.ValueText == "hair_color" )
					ParseHairColours( p, child );
				else if( id.ValueText == "eye_color" )
					ParseEyeColours( p, child );
			}

			if( !PortraitTypes.ContainsKey( p.Name ) )
				PortraitTypes.Add( p.Name, p );
			else
				Errors.Add( "Portrait type " + p.Name + " has already been defined." );
		}

		private void ParseEyeColours( PortraitType p, SyntaxTreeNode node )
		{
			Colour c;
			IEnumerable<SyntaxTreeNode> children = node.Children.Where( child => child.Symbol.Name == "colourGroup" );
			SymbolTokenText value;

			foreach( SyntaxTreeNode child in children )
			{
				Log( " --Parsing Eye Colour" );

				c = new Colour();

				value = child.Children[0].Symbol as SymbolTokenText;
				c.Red = byte.Parse( value.ValueText );
				value = child.Children[1].Symbol as SymbolTokenText;
				c.Green = byte.Parse( value.ValueText );
				value = child.Children[2].Symbol as SymbolTokenText;
				c.Blue = byte.Parse( value.ValueText );

				Log( "   --Colour: " + c.Red + " " + c.Green + " " + c.Blue );

				p.EyeColours.Add( c );
			}
		}

		private void ParseHairColours( PortraitType p, SyntaxTreeNode node )
		{
			Colour c;
			Hair h;
			List<SyntaxTreeNode> children = node.Children.Where( child => child.Symbol.Name == "colourGroup" ).ToList();
			SymbolTokenText value;
			SyntaxTreeNode token;

			for( int i = 0; i < children.Count; i += 3 )
			{
				Log( " --Parsing Hair Colour" );

				h = new Hair();

				c = new Colour();
				token = children[i];
				value = token.Children[0].Symbol as SymbolTokenText;
				c.Red = byte.Parse( value.ValueText );
				value = token.Children[1].Symbol as SymbolTokenText;
				c.Green = byte.Parse( value.ValueText );
				value = token.Children[2].Symbol as SymbolTokenText;
				c.Blue = byte.Parse( value.ValueText );
				h.Dark = c;

				Log( "   --Dark: " + c.Red + " " + c.Green + " " + c.Blue );

				c = new Colour();
				token = children[i + 1];
				value = token.Children[0].Symbol as SymbolTokenText;
				c.Red = byte.Parse( value.ValueText );
				value = token.Children[1].Symbol as SymbolTokenText;
				c.Green = byte.Parse( value.ValueText );
				value = token.Children[2].Symbol as SymbolTokenText;
				c.Blue = byte.Parse( value.ValueText );
				h.Base = c;

				Log( "   --Base: " + c.Red + " " + c.Green + " " + c.Blue );

				c = new Colour();
				token = children[i + 2];
				value = token.Children[0].Symbol as SymbolTokenText;
				c.Red = byte.Parse( value.ValueText );
				value = token.Children[1].Symbol as SymbolTokenText;
				c.Green = byte.Parse( value.ValueText );
				value = token.Children[2].Symbol as SymbolTokenText;
				c.Blue = byte.Parse( value.ValueText );
				h.Highlight = c;

				Log( "   --Highlight: " + c.Red + " " + c.Green + " " + c.Blue );

				p.HairColours.Add( h );
			}
		}

		private void ParseLayers( PortraitType p, SyntaxTreeNode node, string filename )
		{
			string[] s;
			Layer l;

			foreach( SyntaxTreeNode child in node.Children )
			{
				Log( " --Parsing Layer" );

				s = ( (SymbolTokenText)child.Symbol ).ValueText.Replace( "\"", "" ).Split( ':' );

				l = new Layer();
				l.Filename = filename;

				l.Name = s[0];
				l.LayerType = s[1][0] == 'd' ? Layer.Type.DNA : Layer.Type.Property;
				l.Index = int.Parse( s[1].Substring( 1 ) );

				if( s.Length >= 3 )
				{
					if( s[2] == "h" || s[2] == "x" )
						l.IsHair = true;
					else if( s[2] == "e" )
						l.IsEye = true;
				}
				if( s.Length == 4 )
					l.DontRefreshIfValid = s[3] == "y";

				Log( "   --ID: " + l.Name );
				Log( "   --Index: " + l.Index );
				Log( "   --Layer Type: " + ( l.LayerType == Layer.Type.DNA ? "DNA" : "Property" ) );
				Log( "   --Is Hair: " + l.IsHair );
				Log( "   --Is Eye: " + l.IsEye );

				p.Layers.Add( l );
			}
		}

		private void ParseSpriteType( SyntaxTreeNode node, string filename )
		{
			Sprite s = new Sprite();
			s.Filename = filename;

			IEnumerable<SyntaxTreeNode> children = node.Children.Where( child => child.Symbol.Name == "Option" );
			SymbolTokenText id, value;
			SyntaxTreeNode token;
			foreach( SyntaxTreeNode child in children )
			{
				token = child.Children[0];

				if( token.Children.Count > 1 == false )
					continue;

				id = token.Children[0].Symbol as SymbolTokenText;
				value = token.Children[1].Symbol as SymbolTokenText;

				switch( token.Symbol.Name )
				{
					case "stringOption":
						if( id.ValueText == "name" )
							s.Name = value.ValueText.Replace( "\"", "" );
						if( id.ValueText == "texturefile" )
							s.TextureFilePath = value.ValueText.Replace( "\"", "" ).Replace( @"\\", @"\" );
						break;
					case "boolOption":
						if( id.ValueText == "norefcount" )
							s.NoRefCount = value.ValueText == "yes";
						break;
					case "numberOption":
						if( id.ValueText == "noOfFrames" )
							s.FrameCount = Int32.Parse( value.ValueText );
						break;
				}
			}
			Log( "Sprite Parsed: " );
			Log( " --ID: " + s.Name );
			Log( " --Texture File: " + s.TextureFilePath );
			Log( " --NoRefCount: " + s.NoRefCount );
			Log( " --Frame Count: " + s.FrameCount );

			if( Sprites.ContainsKey( s.Name ) )
			{
				Log( "Sprite already exists. Replacing." );
				Sprites.Remove( s.Name );
			}

			Sprites.Add( s.Name, s );
		}

		/// <summary>
		/// Draws a character portrait.
		/// </summary>
		/// <param name="ckDir">Path of the Crusader Kings II directory. E.g. C:\Paradox\Crusader Kings II</param>
		/// <param name="pType">PortaitType to use for drawing.</param>
		/// <param name="dna">DNA string to use for drawing.</param>
		/// <param name="properties">Properties string to use for drawing.</param> 
		/// <param name="myDocsDir">Fath to the My Documents directory.</param>
		/// <returns>Frameless portrait drawn with the given parameters.</returns>
		public Bitmap DrawPortrait( string ckDir, PortraitType pType, string dna, string properties, string myDocsDir )
		{
			return DrawPortrait( ckDir, new Mod.Mod
			{
				ModPathType = ModReader.Folder.CKDir,
				Name = "Vanilla"
			}, pType, dna, properties, myDocsDir );
		}

		/// <summary>
		/// Draws a character portrait.
		/// </summary>
		/// <param name="ckDir">Path of the Crusader Kings II directory. E.g. C:\Paradox\Crusader Kings II</param>
		/// <param name="selMod">Mod object to use when drawing.</param>
		/// <param name="pType">PortaitType to use for drawing.</param>
		/// <param name="dna">DNA string to use for drawing.</param>
		/// <param name="properties">Properties string to use for drawing.</param>
		/// <param name="myDocsDir">Fath to the My Documents directory.</param>
		/// <returns>Frameless portrait drawn with the given parameters.</returns>
		public Bitmap DrawPortrait( string ckDir, Mod.Mod selMod, PortraitType pType, string dna, string properties, string myDocsDir )
		{
			DrawErrors = new List<string>();

			Log( "Drawing Portrait" );
			Log( "  --DNA: " + dna );
			Log( "  --Properties: " + properties );
			Log( "  --CKDir: " + ckDir );
			Log( "  --Documents Dir: " + myDocsDir );
			Log( "  --Mod: " + selMod.Name );

			if( dna.Length < 9 || properties.Length < 9)
			{
				DrawErrors.Add( "DNA or Property strings are too short." );
				Log( "  --Error: DNA or Portrait string too short." );
				return null;
			}

			Bitmap portrait = new Bitmap( 176, 176 ), hairEyeTemp;
			Graphics g = Graphics.FromImage( portrait );
			char letter;
			Sprite sprite;
			int tileIndex, hairEyeIndex;

			string dir = selMod.ModPathType == ModReader.Folder.CKDir ? ckDir : myDocsDir;

			foreach( Layer l in pType.Layers )
			{
                Log("--Drawing Layer " + l.Index);

                try {

				if( Sprites.ContainsKey( l.Name ) )
				{
					Log( "  --Searching for sprite ID: " + l.Name );
					sprite = Sprites[l.Name];
				} else
				{
					Log( "  --Sprite not found." );
					DrawErrors.Add( "Sprite not found: " + l.Name );
					continue;
				}

				//Check if loaded; if not, then load
				if( !sprite.IsLoaded )
				{
					if( File.Exists( dir + "/" + selMod.Path + "/" + sprite.TextureFilePath ) )
					{
						Log( "  --Loading sprite from: " + dir + "/" + selMod.Path + "/" + sprite.TextureFilePath );
						sprite.Load( dir + "/" + selMod.Path );
					} else if( File.Exists( ckDir + "/" + sprite.TextureFilePath ) )
					{
						Log( "  --Loading sprite from: " + ckDir + "/" + sprite.TextureFilePath );
						sprite.Load( ckDir + "/" );
					} else
					{
						Log( "  --Error: Unable to find file: " + sprite.TextureFilePath );
						DrawErrors.Add( "Unable to find file: " + sprite.TextureFilePath );
						continue;
					}
				}

				//Get DNA/Properties letter, then the index of the tile to draw
				letter = l.LayerType == Layer.Type.DNA ? dna[l.Index] : properties[l.Index];
				Log( "  --Layer Type: " + ( l.LayerType == Layer.Type.DNA ? "DNA" : "Property" ) );
				Log( "  --Layer Index: " + l.Index );
				Log( "  --Layer Letter: " + letter );
				tileIndex = GetIndex( letter, sprite.FrameCount );
				Log( "  --Tile Index: " + tileIndex );

				//Draw tile
				if( l.IsHair )
				{
					Log( "  --Drawing Layer as Hair" );
					letter = dna[pType.HairColourIndex];
					hairEyeIndex = GetIndex( letter, pType.HairColours.Count );
					hairEyeTemp = DrawHair( sprite.Tiles[tileIndex], pType.HairColours[hairEyeIndex] );
					g.DrawImage( hairEyeTemp, 12, 12 );
				} else if( l.IsEye )
				{
					Log( "  --Drawing Layer as Eye" );
					letter = dna[pType.EyeColourIndex];
					hairEyeIndex = GetIndex( letter, pType.EyeColours.Count );
					hairEyeTemp = DrawEye( sprite.Tiles[tileIndex], pType.EyeColours[hairEyeIndex] );
					g.DrawImage( hairEyeTemp, 12, 12 );
				} else
				{
					Log( "  --Drawing Layer" );
					g.DrawImage( sprite.Tiles[tileIndex], 12, 12 );
				}
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Could not render layer" + l.Index);
                    System.Diagnostics.Debug.WriteLine(e);
                }
			}

			g.Dispose();
			return portrait;
		}

		private Bitmap DrawEye( Bitmap b, Colour ec )
		{
			Bitmap output = new Bitmap( 152, 152 );
			Colour c1 = new Colour(), c2 = new Colour();

			BitmapData bdata = b.LockBits( new Rectangle( 0, 0, 152, 152 ), ImageLockMode.ReadOnly, b.PixelFormat );
			BitmapData odata = output.LockBits( new Rectangle( 0, 0, 152, 152 ), ImageLockMode.ReadOnly, output.PixelFormat );
			int pixelSize = 4;

			unsafe
			{
				for( int y = 0; y < 152; y++ )
				{
					byte* brow = (byte*)bdata.Scan0 + ( y * bdata.Stride );
					byte* orow = (byte*)odata.Scan0 + ( y * odata.Stride );

					for( int x = 0; x < 152; x++ )
					{
						c1.Red = brow[x * pixelSize + 2];
						c1.Alpha = brow[x * pixelSize + 3];

						if( c1.Alpha > 0 )
						{
							c2.Red = (byte)( 255 * ( ( ec.Red / 255f ) * ( c1.Red / 255f ) ) );
							c2.Green = (byte)( 255 * ( ( ec.Green / 255f ) * ( c1.Red / 255f ) ) );
							c2.Blue = (byte)( 255 * ( ( ec.Blue / 255f ) * ( c1.Red / 255f ) ) );

							orow[x * pixelSize] = c2.Blue;
							orow[x * pixelSize + 1] = c2.Green;
							orow[x * pixelSize + 2] = c2.Red;
							orow[x * pixelSize + 3] = c1.Alpha;
						}
					}
				}
			}

			b.UnlockBits( bdata );
			output.UnlockBits( odata );

			return output;
		}

		private Bitmap DrawHair( Bitmap b, Hair hc )
		{
			Bitmap output = new Bitmap( 152, 152 );
			Colour c1 = new Colour(), c2;

			BitmapData bdata = b.LockBits( new Rectangle( 0, 0, 152, 152 ), ImageLockMode.ReadOnly, b.PixelFormat );
			BitmapData odata = output.LockBits( new Rectangle( 0, 0, 152, 152 ), ImageLockMode.ReadOnly, output.PixelFormat );
			int pixelSize = 4;

			unsafe
			{
				for( int y = 0; y < 152; y++ )
				{
					byte* brow = (byte*)bdata.Scan0 + ( y * bdata.Stride );
					byte* orow = (byte*)odata.Scan0 + ( y * odata.Stride );

					for( int x = 0; x < 152; x++ )
					{
						c1.Green = brow[x * pixelSize + 1];
						c1.Alpha = brow[x * pixelSize + 3];

						if( c1.Alpha > 0 )
						{
							c2 = Colour.Lerp( hc.Dark, hc.Base, Colour.Clamp( c1.Green * 2 ) );
							c2 = Colour.Lerp( c2, hc.Highlight, Colour.Clamp( ( c1.Green - 128 ) * 2 ) );

							orow[x * pixelSize] = c2.Blue;
							orow[x * pixelSize + 1] = c2.Green;
							orow[x * pixelSize + 2] = c2.Red;
							orow[x * pixelSize + 3] = c1.Alpha;
						}
					}
				}
			}

			b.UnlockBits( bdata );
			output.UnlockBits( odata );

			return output;
		}

		private int GetIndex( char letter, int frameCount )
		{
			int index = 0;

			if( letter == '0' )
				return m_rand.Next( frameCount );

			index = Letters.IndexOf( letter ) + 1;
			index = index % frameCount;

			if( index == frameCount )
				index = 0;

			return index;
		}
	}
}
