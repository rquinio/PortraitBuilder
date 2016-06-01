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
		private Random rand = new Random();

		private bool shouldLog = false;
		private StreamWriter log;

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
			this.log = log;
			shouldLog = true;
		}

		private void Log( string text )
		{
            System.Diagnostics.Debug.WriteLine(text);
			if( shouldLog )
			{
				log.WriteLine( "PRC > " + text );
				log.Flush();
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

			StreamReader stream = new StreamReader( filename, Encoding.GetEncoding( 1252 ) );
			string fileContent = stream.ReadToEnd();
			stream.Dispose();

			//Check the file isn't empty
			string[] lines = fileContent.Split( fileContent.Contains( '\r' ) ? '\r' : '\n' );
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
            PortraitReaderLexer lexer = new PortraitReaderLexer(fileContent);
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
			PortraitType portraitType = new PortraitType();
			portraitType.Filename = filename;

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
							portraitType.Name = value.ValueText.Replace( "\"", "" );
						if( id.ValueText == "effectFile" )
							portraitType.EffectFile = value.ValueText.Replace( "\"", "" ).Replace( @"\\", @"\" );
						break;
					case "numberOption":
						if( id.ValueText == "hair_color_index" )
							portraitType.HairColourIndex = Int32.Parse( value.ValueText );
						if( id.ValueText == "eye_color_index" )
							portraitType.EyeColourIndex = Int32.Parse( value.ValueText );
						break;
				}
			}

			Log( "Type parsed: " );
			Log( " --ID: " + portraitType.Name );
			Log( " --Hair Colour Index: " + portraitType.HairColourIndex );
			Log( " --Eye Colour Index: " + portraitType.EyeColourIndex );

			ParseLayers( portraitType, node.Children.Single( c => c.Symbol.Name == "layerGroup" ), filename );

			children = node.Children.Where( c => c.Symbol.Name == "cultureGroup" ).ToList();
			if( children.Count > 0 )
			{
				foreach( SyntaxTreeNode child in children[0].Children )
					portraitType.Culture.Add( Int32.Parse( ( (SymbolTokenText)child.Symbol ).ValueText ) );
			}

			children = node.Children.Where( c => c.Symbol.Name == "groupOption" ).ToList();

			foreach( SyntaxTreeNode child in children )
			{
				id = child.Children[0].Symbol as SymbolTokenText;

				if( id.ValueText == "hair_color" )
					ParseHairColours( portraitType, child );
				else if( id.ValueText == "eye_color" )
					ParseEyeColours( portraitType, child );
			}

			if( !PortraitTypes.ContainsKey( portraitType.Name ) )
				PortraitTypes.Add( portraitType.Name, portraitType );
			else
				Errors.Add( "Portrait type " + portraitType.Name + " has already been defined." );
		}

		private void ParseEyeColours( PortraitType portraitType, SyntaxTreeNode node )
		{
			IEnumerable<SyntaxTreeNode> children = node.Children.Where( child => child.Symbol.Name == "colourGroup" );

			foreach( SyntaxTreeNode child in children )
			{
				portraitType.EyeColours.Add( ParseColour(child) );
			}
		}

		private void ParseHairColours( PortraitType portraitType, SyntaxTreeNode node )
		{
			List<SyntaxTreeNode> children = node.Children.Where( child => child.Symbol.Name == "colourGroup" ).ToList();

			for( int i = 0; i < children.Count; i += 3 )
			{
				Log( " --Parsing Hair colours" );

                Hair hair = new Hair();

                hair.Dark = ParseColour(children[i]);
                Log("   --Dark: " + hair.Dark.Red + " " + hair.Dark.Green + " " + hair.Dark.Blue);

                hair.Base = ParseColour(children[i + 1]);
                Log("   --Base: " + hair.Base.Red + " " + hair.Base.Green + " " + hair.Base.Blue);

                hair.Highlight = ParseColour(children[i + 2]);
                Log("   --Highlight: " + hair.Highlight.Red + " " + hair.Highlight.Green + " " + hair.Highlight.Blue);

				portraitType.HairColours.Add( hair );
			}
		}

        private Colour ParseColour(SyntaxTreeNode child)
        {
            Log(" --Parsing Colour");

            Colour colour = new Colour();

            SymbolTokenText value = child.Children[0].Symbol as SymbolTokenText;
            colour.Red = byte.Parse(value.ValueText);
            value = child.Children[1].Symbol as SymbolTokenText;
            colour.Green = byte.Parse(value.ValueText);
            value = child.Children[2].Symbol as SymbolTokenText;
            colour.Blue = byte.Parse(value.ValueText);

            Log("   --Colour: " + colour.Red + " " + colour.Green + " " + colour.Blue);

            return colour;
        }

		private void ParseLayers( PortraitType portraitType, SyntaxTreeNode node, string filename )
		{
			foreach( SyntaxTreeNode child in node.Children )
			{
                portraitType.Layers.Add(ParseLayer(portraitType, child, filename));
			}
		}

        private Layer ParseLayer(PortraitType portraitType, SyntaxTreeNode node, string filename)
        {
            Log(" --Parsing Layer");

            string[] layerParts = ((SymbolTokenText)node.Symbol).ValueText.Replace("\"", "").Split(':');

            Layer layer = new Layer();
            layer.Filename = filename;

            layer.Name = layerParts[0];
            layer.LayerType = layerParts[1][0] == 'd' ? Layer.Type.DNA : Layer.Type.Property;
            layer.Index = int.Parse(layerParts[1].Substring(1));

            for (int i = 2; i < layerParts.Length; i++)
            {
                if (layerParts[i] == "h" || layerParts[2] == "x")
                {
                    layer.IsHair = true;
                }
                else if (layerParts[i] == "e")
                {
                    layer.IsEye = true;
                }
                else if (layerParts[i] == "y")
                {
                    layer.DontRefreshIfValid = true;
                }
                else if (layerParts[i].StartsWith("o"))
                {
                    string[] offsets = layerParts[i].Substring(1).Split('x');
                    layer.Offset = new Point(int.Parse(offsets[0]), int.Parse(offsets[1]));
                }
            }

            Log("   --ID: " + layer.Name);
            Log("   --Index: " + layer.Index);
            Log("   --Layer Type: " + (layer.LayerType == Layer.Type.DNA ? "DNA" : "Property"));
            Log("   --Is Hair: " + layer.IsHair);
            Log("   --Is Eye: " + layer.IsEye);
            Log("   --Offset: " + layer.Offset);

            return layer;
        }

		private void ParseSpriteType( SyntaxTreeNode node, string filename )
		{
			Sprite sprite = new Sprite();
			sprite.Filename = filename;

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
							sprite.Name = value.ValueText.Replace( "\"", "" );
						if( id.ValueText == "texturefile" )
							sprite.TextureFilePath = value.ValueText.Replace( "\"", "" ).Replace( @"\\", @"\" );
						break;
					case "boolOption":
						if( id.ValueText == "norefcount" )
							sprite.NoRefCount = value.ValueText == "yes";
						break;
					case "numberOption":
						if( id.ValueText == "noOfFrames" )
							sprite.FrameCount = Int32.Parse( value.ValueText );
						break;
				}
			}
			Log( "Sprite Parsed: " );
			Log( " --ID: " + sprite.Name );
			Log( " --Texture File: " + sprite.TextureFilePath );
			Log( " --NoRefCount: " + sprite.NoRefCount );
			Log( " --Frame Count: " + sprite.FrameCount );

			if( Sprites.ContainsKey( sprite.Name ) )
			{
				Log( "Sprite already exists. Replacing." );
				Sprites.Remove( sprite.Name );
			}

			Sprites.Add( sprite.Name, sprite );
		}

		/// <summary>
		/// Draws a character portrait.
		/// </summary>
		/// <param name="ckDir">Path of the Crusader Kings II directory. E.g. C:\Paradox\Crusader Kings II</param>
		/// <param name="portraitType">PortaitType to use for drawing.</param>
		/// <param name="dna">DNA string to use for drawing.</param>
		/// <param name="properties">Properties string to use for drawing.</param> 
		/// <param name="myDocsDir">Fath to the My Documents directory.</param>
		/// <returns>Frameless portrait drawn with the given parameters.</returns>
		public Bitmap DrawPortrait( string ckDir, PortraitType portraitType, string dna, string properties, string myDocsDir )
		{
			return DrawPortrait( ckDir, new Mod.Mod
			{
				ModPathType = ModReader.Folder.CKDir,
				Name = "Vanilla"
			}, portraitType, dna, properties, myDocsDir );
		}

		/// <summary>
		/// Draws a character portrait.
		/// </summary>
		/// <param name="ckDir">Path of the Crusader Kings II directory. E.g. C:\Paradox\Crusader Kings II</param>
		/// <param name="selectedMod">Mod object to use when drawing.</param>
		/// <param name="portraitType">PortaitType to use for drawing.</param>
		/// <param name="dna">DNA string to use for drawing.</param>
		/// <param name="properties">Properties string to use for drawing.</param>
		/// <param name="myDocsDir">Fath to the My Documents directory.</param>
		/// <returns>Frameless portrait drawn with the given parameters.</returns>
		public Bitmap DrawPortrait( string ckDir, Mod.Mod selectedMod, PortraitType portraitType, string dna, string properties, string myDocsDir )
		{
			DrawErrors = new List<string>();

			Log( "Drawing Portrait" );
			Log( "  --DNA: " + dna );
			Log( "  --Properties: " + properties );
			Log( "  --CKDir: " + ckDir );
			Log( "  --Documents Dir: " + myDocsDir );
			Log( "  --Mod: " + selectedMod.Name );

			if( dna.Length < 9 || properties.Length < 9)
			{
				DrawErrors.Add( "DNA or Property strings are too short." );
				Log( "  --Error: DNA or Portrait string too short." );
				return null;
			}

			Bitmap portrait = new Bitmap( 176, 176 );
			Graphics g = Graphics.FromImage( portrait );
			Sprite sprite;

			string dir = selectedMod.ModPathType == ModReader.Folder.CKDir ? ckDir : myDocsDir;

			foreach( Layer layer in portraitType.Layers )
			{
                Log("--Drawing Layer " + layer.Index);

                try {

				if( Sprites.ContainsKey( layer.Name ) )
				{
					Log( "  --Searching for sprite ID: " + layer.Name );
					sprite = Sprites[layer.Name];
				} else
				{
					Log( "  --Sprite not found." );
					DrawErrors.Add( "Sprite not found: " + layer.Name );
					continue;
				}

				//Check if loaded; if not, then load
				if( !sprite.IsLoaded )
				{
                    string filePath = sprite.TextureFilePath;
                    if (File.Exists(dir + "/" + selectedMod.Path + "/" + filePath))
					{
                        Log("  --Loading sprite from: " + dir + "/" + selectedMod.Path + "/" + filePath);
						sprite.Load( dir + "/" + selectedMod.Path );
                    }
                    else if (File.Exists(ckDir + "/" + filePath))
					{
                        Log("  --Loading sprite from: " + ckDir + "/" + filePath);
						sprite.Load( ckDir + "/" );
					} else
					{
                        Log("  --Error: Unable to find file: " + filePath);
                        DrawErrors.Add("Unable to find file: " + filePath);
						continue;
					}
				}

				//Get DNA/Properties letter, then the index of the tile to draw
                int tileIndex = GetTileIndex(dna, properties, sprite.FrameCount, layer);

                DrawTile(portraitType, dna, g, sprite, layer, tileIndex);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Could not render layer" + layer.Index);
                    System.Diagnostics.Debug.WriteLine(e);
                }
			}

			g.Dispose();
			return portrait;
		}

        private int GetTileIndex(string dna, string properties, int frameCount, Layer layer)
        {
            char letter = layer.LayerType == Layer.Type.DNA ? dna[layer.Index] : properties[layer.Index];
            Log("  --Layer Type: " + (layer.LayerType == Layer.Type.DNA ? "DNA" : "Property"));
            Log("  --Layer Index: " + layer.Index);
            Log("  --Layer Letter: " + letter);
            int tileIndex = GetTileIndexFromLetter(letter, frameCount);
            Log("  --Tile Index: " + tileIndex);
            return tileIndex;
        }

        private void DrawTile(PortraitType portraitType, string dna, Graphics g, Sprite sprite, Layer layer, int tileIndex)
        {
            Bitmap tile;
            if (layer.IsHair)
            {
                Log("  --Drawing Layer as Hair");
                int hairIndex = GetTileIndexFromLetter(dna[portraitType.HairColourIndex], portraitType.HairColours.Count);
                tile = DrawHair(sprite.Tiles[tileIndex], portraitType.HairColours[hairIndex]);

            }
            else if (layer.IsEye)
            {
                Log("  --Drawing Layer as Eye");
                int eyeIndex = GetTileIndexFromLetter(dna[portraitType.EyeColourIndex], portraitType.EyeColours.Count);
                tile = DrawEye(sprite.Tiles[tileIndex], portraitType.EyeColours[eyeIndex]);

            }
            else
            {
                Log("  --Drawing Layer");
                tile = sprite.Tiles[tileIndex];
            }

            g.DrawImage(tile, 12 + layer.Offset.X, 12 + layer.Offset.Y);
        }

		private Bitmap DrawEye( Bitmap source, Colour eyeColour )
		{
			Bitmap output = new Bitmap( 152, 152 );
			Colour colour1 = new Colour(), colour2 = new Colour();

			BitmapData bdata = source.LockBits( new Rectangle( 0, 0, 152, 152 ), ImageLockMode.ReadOnly, source.PixelFormat );
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
						colour1.Red = brow[x * pixelSize + 2];
						colour1.Alpha = brow[x * pixelSize + 3];

						if( colour1.Alpha > 0 )
						{
							colour2.Red = (byte)( 255 * ( ( eyeColour.Red / 255f ) * ( colour1.Red / 255f ) ) );
							colour2.Green = (byte)( 255 * ( ( eyeColour.Green / 255f ) * ( colour1.Red / 255f ) ) );
							colour2.Blue = (byte)( 255 * ( ( eyeColour.Blue / 255f ) * ( colour1.Red / 255f ) ) );

							orow[x * pixelSize] = colour2.Blue;
							orow[x * pixelSize + 1] = colour2.Green;
							orow[x * pixelSize + 2] = colour2.Red;
							orow[x * pixelSize + 3] = colour1.Alpha;
						}
					}
				}
			}

			source.UnlockBits( bdata );
			output.UnlockBits( odata );

			return output;
		}

		private Bitmap DrawHair( Bitmap source, Hair hairColor )
		{
			Bitmap output = new Bitmap( 152, 152 );
			Colour colour1 = new Colour(), colour2;

			BitmapData bdata = source.LockBits( new Rectangle( 0, 0, 152, 152 ), ImageLockMode.ReadOnly, source.PixelFormat );
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
						colour1.Green = brow[x * pixelSize + 1];
						colour1.Alpha = brow[x * pixelSize + 3];

						if( colour1.Alpha > 0 )
						{
							colour2 = Colour.Lerp( hairColor.Dark, hairColor.Base, Colour.Clamp( colour1.Green * 2 ) );
							colour2 = Colour.Lerp( colour2, hairColor.Highlight, Colour.Clamp( ( colour1.Green - 128 ) * 2 ) );

							orow[x * pixelSize] = colour2.Blue;
							orow[x * pixelSize + 1] = colour2.Green;
							orow[x * pixelSize + 2] = colour2.Red;
							orow[x * pixelSize + 3] = colour1.Alpha;
						}
					}
				}
			}

			source.UnlockBits( bdata );
			output.UnlockBits( odata );

			return output;
		}

		private int GetTileIndexFromLetter( char letter, int frameCount )
		{
			int index = 0;

			if( letter == '0' )
				return rand.Next( frameCount );

			index = Letters.IndexOf( letter ) + 1;
			index = index % frameCount;

			if( index == frameCount )
				index = 0;

			return index;
		}
	}
}
