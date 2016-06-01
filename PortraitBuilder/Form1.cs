using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using Parsers.DLC;
using Parsers.Mod;
using Parsers.Portrait;

namespace Portrait_Builder
{
  public partial class Form1 : Form
  {
    private Image previewImage = new Bitmap( 176, 176 );
    private bool started = false;
    private StringBuilder dnaPropOutput;
    public static Random rand = new Random();

    private string ck2Dir = string.Empty;
    private string myDocsDir = string.Empty;
    private string dlcDir = string.Empty;

    private Mod selectedMod = new Mod();
    private List<Mod> usableMods = new List<Mod>();
    private PortraitReader portraitReader = new PortraitReader();
    private List<Bitmap> borders = new List<Bitmap>();
    private string dna, properties;

    private StreamWriter log;
    private MemoryStream memoryStream;
    private bool forceLog = false;
    private bool fullLogging = false;
    private string version = string.Empty;
    private bool hadError = false;
    private StringBuilder modListSetup;


    public Form1()
    {
      InitializeComponent(); 
      EnvironmentSetup();
    }

    public Form1( bool logging, bool full )
    {
      InitializeComponent();
      
      forceLog = logging;
      fullLogging = full;   
      
      EnvironmentSetup();
    }

    private void EnvironmentSetup()
    {
      myDocsDir = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) +
                    @"\Paradox Interactive\Crusader Kings II\";
      dlcDir = Environment.CurrentDirectory + @"\dlc";
      ReadDir();
      modListSetup = new StringBuilder();
      LoadDLC();
      LoadMods();
      
      Assembly assembly = Assembly.GetExecutingAssembly();
      FileVersionInfo fvi = FileVersionInfo.GetVersionInfo( assembly.Location );
      version = fvi.FileVersion;

      SetupStream();

      LoadPortraits();
      LoadBorders();

      if( hadError )
        return;

      SetupSharedUI();
      SetupUI();
      RandomizeUI( true );
      UpdatePortrait();

      started = true;
    }

    private void SetupStream()
    {
      memoryStream = new MemoryStream();
      log = new StreamWriter( memoryStream );

      if ( fullLogging ) portraitReader.SetLogging( log );

      Log( "Portrait Builder Version " + version );
      Log( "Portrait Builder Parser Library " + Parsers.Version.GetVersion() );
      Log( "Logging started at " + DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" ) );
      Log( "" );

      Log( modListSetup.ToString() );
    }

    private void DumpLog()
    {
      Stream stream = new FileStream( "log.txt", FileMode.Create, FileAccess.Write );
      memoryStream.WriteTo( stream );
      stream.Close();
      stream.Dispose();
    }

    private void Log( string text )
    {
        System.Diagnostics.Debug.WriteLine(text);
      log.WriteLine( text );
      log.Flush();
    }

    private void LoadBorders()
    {
      Log( "Setting up borders." );
      if( !File.Exists( ck2Dir + @"\gfx\interface\charframe_150.dds" ) )
      {
        Log( "Error: Borders file \\gfx\\interface\\charframe_150.dds not found." );
        hadError = true;
        DumpLog();

        MessageBox.Show( this, "Unable to find borders graphic.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
        return;
      }

      Bitmap charFrame = DevIL.DevIL.LoadBitmap( ck2Dir + @"\gfx\interface\charframe_150.dds" );

      for( int i = 0; i < 6; i++ )
      {
        Bitmap border = new Bitmap(176, 176);
        Graphics g = Graphics.FromImage(border);
        g.DrawImage( charFrame, 0, 0, new Rectangle( i * 176, 0, 176, 176 ), GraphicsUnit.Pixel );
        g.Dispose();
        borders.Add( border );
      }
    }

    private void LoadPortraits()
    {
      Log( "Disposing of previous portrait data." );
      portraitReader.Dispose();

      if( cbModEnable.Checked &&
          selectedMod.CustomFlags.ContainsKey( "HasPortrait" ) &&
          (bool)selectedMod.CustomFlags["HasPortrait"] )
      {
        Log( "Loading portraits from mod: " + selectedMod.Name );

        string dir = string.Empty;
        switch ( selectedMod.ModPathType )
        {
          case ModReader.Folder.CKDir:
            dir = ck2Dir;
            break;
          case ModReader.Folder.MyDocs:
            dir = myDocsDir;
            break;
          case ModReader.Folder.DLC:
            dir = dlcDir;
            break;
        }

        string[] fileNames = Directory.GetFiles( dir + @"\" + selectedMod.Path + @"\interface\", "*.gfx" );

        foreach( string fileName in fileNames )
          portraitReader.Parse( fileName );
      } else
      {
        Log( "Loading portraits from vanilla." );

        string[] fileNames = Directory.GetFiles(ck2Dir + @"\interface\", "*.gfx");

        foreach (string fileName in fileNames)
            portraitReader.Parse(fileName);
      }

      Log( "The following errors were encountered while loading." );

      foreach( string error in portraitReader.Errors )
        Log( " --" + error );

      cbPortraitTypes.Items.Clear();

      if( portraitReader.PortraitTypes.Count == 0 )
      {
        Log( "Error: No portrait types found." );

        MessageBox.Show( this, "No portraits found in mod " + selectedMod.Name + "\n\nCheck errorlog.txt for details.", "Error", MessageBoxButtons.OK,
                         MessageBoxIcon.Error );

        DumpLog();

        hadError = true;

        return;
      }
       
      Log( "Setting up type flags" );
      foreach( KeyValuePair<string, PortraitType> pair in portraitReader.PortraitTypes )
      {
        Log( " --Setting up flags for " + pair.Value.Name );
        cbPortraitTypes.Items.Add( pair.Value.Name );

        foreach( Layer layer in pair.Value.Layers )
        {
          #region shared
          if( layer.Name.Contains( "background" ) && !pair.Value.CustomFlags.ContainsKey( "background" ) )
            pair.Value.CustomFlags.Add( "background", layer.Name );

          if( layer.Name.Contains( "boils" ) && !pair.Value.CustomFlags.ContainsKey( "boils" ) )
            pair.Value.CustomFlags.Add( "boils", layer.Name );

          if( layer.Name.Contains( "reddots" ) && !pair.Value.CustomFlags.ContainsKey( "reddots" ) )
            pair.Value.CustomFlags.Add( "reddots", layer.Name );

          if( layer.Name.Contains( "scars" ) && !pair.Value.CustomFlags.ContainsKey( "scars" ) )
            pair.Value.CustomFlags.Add( "scars", layer.Name );

          if( layer.Name.Contains( "imprisoned" ) && !pair.Value.CustomFlags.ContainsKey( "imprisoned" ) )
            pair.Value.CustomFlags.Add( "imprisoned", layer.Name );

          if( layer.Name.Contains( "blinded" ) && !pair.Value.CustomFlags.ContainsKey( "blinded" ) )
            pair.Value.CustomFlags.Add( "blinded", layer.Name );
          #endregion

          #region Properties
          if( layer.Name.Contains( "clothes" ) && !pair.Value.CustomFlags.ContainsKey( "clothes" ) )
            pair.Value.CustomFlags.Add( "clothes", layer.Name );

          if( layer.Name.Contains( "headgear" ) && !pair.Value.CustomFlags.ContainsKey( "headgear" ) )
            pair.Value.CustomFlags.Add( "headgear", layer.Name );

          if( layer.Name.Contains( "beard" ) && !pair.Value.CustomFlags.ContainsKey( "beard" ) )
            pair.Value.CustomFlags.Add( "beard", layer.Name );

          if( layer.Name.Contains( "hair" ) && !pair.Value.CustomFlags.ContainsKey( "hair" ) )
            pair.Value.CustomFlags.Add( "hair", layer.Name );
          #endregion

          #region DNA
          if( layer.Name.Contains( "base" ) && !pair.Value.CustomFlags.ContainsKey( "base" ) )
            pair.Value.CustomFlags.Add( "base", layer.Name );

          if( layer.Name.Contains( "neck" ) && !pair.Value.CustomFlags.ContainsKey( "neck" ) )
            pair.Value.CustomFlags.Add( "neck", layer.Name );

          if( layer.Name.Contains( "cheeks" ) && !pair.Value.CustomFlags.ContainsKey( "cheeks" ) )
            pair.Value.CustomFlags.Add( "cheeks", layer.Name );

          if( layer.Name.Contains( "chin" ) && !pair.Value.CustomFlags.ContainsKey( "chin" ) )
            pair.Value.CustomFlags.Add( "chin", layer.Name );

          if( layer.Name.Contains( "mouth" ) && !pair.Value.CustomFlags.ContainsKey( "mouth" ) )
            pair.Value.CustomFlags.Add( "mouth", layer.Name );

          if( layer.Name.Contains( "nose" ) && !pair.Value.CustomFlags.ContainsKey( "nose" ) )
            pair.Value.CustomFlags.Add( "nose", layer.Name );

          if( layer.Name.Contains( "eyes" ) && !pair.Value.CustomFlags.ContainsKey( "eyes" ) )
            pair.Value.CustomFlags.Add( "eyes", layer.Name );

          if( layer.Name.Contains( "ear" ) && !pair.Value.CustomFlags.ContainsKey( "ear" ) )
            pair.Value.CustomFlags.Add( "ear", layer.Name );
          #endregion
        }
      }
      cbPortraitTypes.SelectedIndex = 0;
    }

    private void LoadMods()
    {
      ModReader modReader = new ModReader();
      modListSetup.AppendLine( "Loading mods from " + ck2Dir + @"\mod\" );
      modReader.ParseFolder( ck2Dir + @"\mod\", ModReader.Folder.CKDir );
      modListSetup.AppendLine( "Loading mods from " + myDocsDir + @"\mod\" );
      modReader.ParseFolder( myDocsDir + @"\mod\", ModReader.Folder.MyDocs );

      foreach( Mod mod in modReader.Mods )
      {
        string dir = mod.ModPathType == ModReader.Folder.CKDir ? ck2Dir : myDocsDir;

        if( !Directory.Exists( dir + mod.Path ) )
          continue;

        if( File.Exists( dir + mod.Path + @"\interface\portraits.gfx" ) )
        {
          mod.CustomFlags.Add( "HasPortrait", true );
          usableMods.Add( mod );
          continue;
        }

        if( Directory.Exists( dir + mod.Path + @"\gfx\characters\" ) )
        {
          if( Directory.GetDirectories( dir + mod.Path + @"\gfx\characters\" ).Length > 0 )
          {
            mod.CustomFlags.Add( "HasPortrait", false );
            usableMods.Add( mod );
          }
        }
      }

      modListSetup.AppendLine( " --Usable mods found:" );
      if( usableMods.Count > 0 )
      {
        cbModEnable.Enabled = true;
        foreach( Mod mod in usableMods )
        {
          modListSetup.AppendLine( "   --" + mod.Name );
          cbMods.Items.Add( mod.Name );
          cbMods.SelectedIndex = 0;
        }
      }
    }

    private void LoadDLC()
    {
      DLCReader dlcReader = new DLCReader();
      modListSetup.AppendLine( "Loading DLC from " + ck2Dir + @"\dlc\" );
      dlcReader.ParseFolder( ck2Dir + @"\dlc" );

      FastZip fastZip = new FastZip();
      foreach ( DLC dlc in dlcReader.DLCs )
        fastZip.ExtractZip( ck2Dir + dlc.Archive, dlcDir, null );

      if ( !Directory.Exists( dlcDir + @"\gfx\characters\" ) )
        return;

      Mod mod = new Mod();
      mod.ModPathType = ModReader.Folder.DLC;
      mod.Name = "DLC Portraits";
      mod.Path = string.Empty;
      mod.CustomFlags.Add( "HasPortrait", true );
      usableMods.Add( mod );
    }

    private void ReadDir()
    {
        Stream stream = new FileStream("gamedir", FileMode.Open);
        BinaryReader reader = new BinaryReader(stream);
      {
        ck2Dir = reader.ReadString() + @"\";
      }
    }

    private void UpdatePortrait()
    {
      Log( "Updating portrait." );

      GetDNA();
      DrawPortrait();
      OutputDNA();
    }

    private void DrawPortrait()
    {
      Log( " --Drawing portrait." );

      Graphics g = Graphics.FromImage( previewImage );

      Log( "   --Clearing preview." );
      g.Clear( Color.Empty );
      Bitmap portrait;
      Log( "   --Rendering portrait." );
      try
      {
        if( cbModEnable.Checked )
        {
          portrait = portraitReader.DrawPortrait( ck2Dir,
                                      selectedMod,
                                      portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()],
                                      dna,
                                      properties,
                                      myDocsDir );
        } else
        {
          portrait = portraitReader.DrawPortrait( ck2Dir,
                                      portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()],
                                      dna,
                                      properties,
                                      myDocsDir );
        }
      } catch( Exception e )
      {
        Log( "Error encountered rendering portrait:" );
        Log( e.ToString() );

        MessageBox.Show( this, "Error rendering portrait:\n\n" + e, "Error", MessageBoxButtons.OK,
                         MessageBoxIcon.Error );

        return;
      }
      g.DrawImage( portrait, 0, 0 );
      Log( "   --Drawing border." );
      g.DrawImage( borders[cbRank.SelectedIndex], 0, 0 );

      if ( portraitReader.DrawErrors.Count > 0 )
      {
        Log( "Errors encountered while rendering portrait:" );
        foreach ( string error in portraitReader.DrawErrors )
          Log( " --" + error );
      }

      pbPortrait.Image = previewImage;
    }

    private void GetDNA()
    {
      Log( " --Building DNA string." );
      StringBuilder sb = new StringBuilder();

      sb.Append( GetLetter( cbNeck ) );
      sb.Append( GetLetter( cbChin ) );
      sb.Append( GetLetter( cbMouth ) );
      sb.Append( GetLetter( cbNose ) );
      sb.Append( GetLetter( cbCheeks ) );
      sb.Append( "0" ); //Head(Unused)
      sb.Append( GetLetter( cbEyes ) );
      sb.Append( GetLetter( cbEars ) );
      sb.Append( GetLetter( cbHairColour ) );
      sb.Append( GetLetter( cbEyeColour ) );
      sb.Append( "0" );

      dna = sb.ToString();

      Log( " --Building Properties string." );
      sb = new StringBuilder();

      sb.Append( GetLetter( cbBackground ) );
      sb.Append( GetLetter( cbHair ) );
      sb.Append( "0" ); //Base(Unused)
      sb.Append( GetLetter( cbClothes ) );
      sb.Append( GetLetter( cbBeard ) );
      sb.Append( GetLetter( cbHeadgear ) );
      sb.Append( GetLetter( cbPrisoner ) ); //Imprisoned
      sb.Append( GetLetter( cbScars ) ); //Scars
      sb.Append( GetLetter( cbRedDots ) ); //Red Dots
      sb.Append( GetLetter( cbBoils ) ); //Boils
      sb.Append( GetLetter( cbBlinded ) );
      sb.Append("0"); // Player overlay

      properties = sb.ToString();
    }

    private void OutputDNA()
    {
      Log( " --Outputting DNA and Property strings." );
      dnaPropOutput = new StringBuilder();

      dnaPropOutput.Append( "  dna=\"" );
      dnaPropOutput.Append( dna );
      dnaPropOutput.AppendLine( "\"" );

      dnaPropOutput.Append( "  properties=\"" );
      dnaPropOutput.Append( properties );
      dnaPropOutput.AppendLine( "\"" );
      tbDNA.Text = dnaPropOutput.ToString();
    }

    public char GetLetter( ComboBox cb )
    {
      char letter;

      if( cb.SelectedIndex == 0 )
        letter = portraitReader.Letters[cb.Items.Count - 1];
      else if( cb.SelectedIndex == -1 )
        letter = '0';
      else
        letter = portraitReader.Letters[cb.SelectedIndex - 1];

      return letter;
    }

    private void RandomizeUI( bool doRank )
    {
      Log( "Randomizing UI" );
      if( doRank )
        cbRank.SelectedIndex = rand.Next( cbRank.Items.Count - 1 );

      cbBackground.SelectedIndex = rand.Next( cbBackground.Items.Count - 1 );
      cbScars.SelectedIndex = rand.Next( cbScars.Items.Count - 1 );
      cbRedDots.SelectedIndex = 0;
      cbBoils.SelectedIndex = 0;
      cbPrisoner.SelectedIndex = 0;
      if( cbBlinded.Items.Count > 0 )
      {
        cbBlinded.SelectedIndex = 0;
        cbBlinded.Enabled = true;
      } else
      {
        cbBlinded.Enabled = false;
        cbBlinded.SelectedIndex = -1;
      }
      cbClothes.SelectedIndex = rand.Next( cbClothes.Items.Count - 1 );
      cbHeadgear.SelectedIndex = rand.Next( cbHeadgear.Items.Count - 1 );
      cbHair.SelectedIndex = rand.Next( cbHair.Items.Count - 1 );
      cbBeard.SelectedIndex = rand.Next( cbBeard.Items.Count - 1 );
      cbNeck.SelectedIndex = rand.Next( cbNeck.Items.Count - 1 );
      cbCheeks.SelectedIndex = rand.Next( cbCheeks.Items.Count - 1 );
      cbChin.SelectedIndex = rand.Next( cbChin.Items.Count - 1 );
      cbMouth.SelectedIndex = rand.Next( cbMouth.Items.Count - 1 );
      cbNose.SelectedIndex = rand.Next( cbNose.Items.Count - 1 );
      cbEyes.SelectedIndex = rand.Next( cbEyes.Items.Count - 1 );
      cbEars.SelectedIndex = rand.Next( cbEars.Items.Count - 1 );
      cbHairColour.SelectedIndex = rand.Next( cbHairColour.Items.Count - 1 );
      cbEyeColour.SelectedIndex = rand.Next( cbEyeColour.Items.Count - 1 );
    }

    private void FillComboBox( ComboBox cb, int count )
    {
      for( int i = 0; i < count; i++ )
        cb.Items.Add( i );
    }

    private void SetupSharedUI()
    {
      Log( "Setting up Shared UI" );

      cbBackground.Items.Clear();
      cbScars.Items.Clear();
      cbRedDots.Items.Clear();
      cbBoils.Items.Clear();
      cbPrisoner.Items.Clear();
      cbBlinded.Items.Clear();

      string spriteName;
      Sprite sprite;

      if( portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags.ContainsKey( "background" ) )
      {
        spriteName = (string)portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags["background"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Background item count: " + sprite.FrameCount );
        FillComboBox( cbBackground, sprite.FrameCount );
      } else
      {
        Log( " --No background flag found, setting UI to 26." );
        FillComboBox( cbBackground, 26 );
      }

      if( portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags.ContainsKey( "scars" ) )
      {
        spriteName = (string)portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags["scars"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Scars item count: " + sprite.FrameCount );
        FillComboBox( cbScars, sprite.FrameCount );
      } else
      {
        Log( " --No scars flag found, setting UI to 26." );
        FillComboBox( cbScars, 26 );
      }

      if( portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags.ContainsKey( "reddots" ) )
      {
        spriteName = (string)portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags["reddots"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Reddots item count: " + sprite.FrameCount );
        FillComboBox( cbRedDots, sprite.FrameCount );
      } else
      {
        Log( " --No reddots flag found, setting UI to 26." );
        FillComboBox( cbRedDots, 26 );
      }

      if( portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags.ContainsKey( "boils" ) )
      {
        spriteName = (string)portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags["boils"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Boils item count: " + sprite.FrameCount );
        FillComboBox( cbBoils, sprite.FrameCount );
      } else
      {
        Log( " --No boils flag found, setting UI to 26." );
        FillComboBox( cbBoils, 26 );
      }

      if( portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags.ContainsKey( "imprisoned" ) )
      {
        spriteName = (string)portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags["imprisoned"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Imprisoned item count: " + sprite.FrameCount );
        FillComboBox( cbPrisoner, sprite.FrameCount );
      } else
      {
        Log( " --No imprisoned flag found, setting UI to 26." );
        FillComboBox( cbPrisoner, 26 );
      }

      if( portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags.ContainsKey( "blinded" ) )
      {
        spriteName = (string)portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags["blinded"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Blinded item count: " + sprite.FrameCount );
        FillComboBox( cbBlinded, sprite.FrameCount );
      } else
      {
        Log( " --No blinded flag found, setting UI to 26." );
        FillComboBox( cbBlinded, 26 );
      }
    }

    private void SetupUI()
    {
      cbClothes.Items.Clear();
      cbHeadgear.Items.Clear();
      cbHair.Items.Clear();
      cbBeard.Items.Clear();
      cbNeck.Items.Clear();
      cbCheeks.Items.Clear();
      cbChin.Items.Clear();
      cbMouth.Items.Clear();
      cbNose.Items.Clear();
      cbEyes.Items.Clear();
      cbEars.Items.Clear();
      cbHairColour.Items.Clear();
      cbEyeColour.Items.Clear();

      Sprite sprite;
      string spriteName;
      PortraitType portraitType = portraitReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()];

      Log( "Setting up UI for: " + portraitType.Name );

      if( portraitType.CustomFlags.ContainsKey( "clothes" ) )
      {
        spriteName = (string)portraitType.CustomFlags["clothes"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Clothes item count: " + sprite.FrameCount );
        FillComboBox( cbClothes, sprite.FrameCount );
      } else
      {
        Log( " --No clothes flag found, setting UI to 26." );
        FillComboBox( cbClothes, 26 );
      }

      if( portraitType.CustomFlags.ContainsKey( "headgear" ) )
      {
        spriteName = (string)portraitType.CustomFlags["headgear"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Headgear item count: " + sprite.FrameCount );
        FillComboBox( cbHeadgear, sprite.FrameCount );
      } else
      {
        Log( " --No headgear flag found, setting UI to 26." );
        FillComboBox( cbHeadgear, 26 );
      }

      if( portraitType.CustomFlags.ContainsKey( "hair" ) )
      {
        spriteName = (string)portraitType.CustomFlags["hair"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Hair item count: " + sprite.FrameCount );
        FillComboBox( cbHair, sprite.FrameCount );
      } else
      {
        Log( " --No hair flag found, setting UI to 26." );
        FillComboBox( cbHair, 26 );
      }

      if( portraitType.CustomFlags.ContainsKey( "beard" ) )
      {
        spriteName = (string)portraitType.CustomFlags["beard"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Beard item count: " + sprite.FrameCount );
        FillComboBox( cbBeard, sprite.FrameCount );
      } else
      {
        Log( " --No beard flag found, setting UI to 26." );
        FillComboBox( cbBeard, 26 );
      }

      if( portraitType.CustomFlags.ContainsKey( "neck" ) )
      {
        spriteName = (string)portraitType.CustomFlags["neck"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Neck item count: " + sprite.FrameCount );
        FillComboBox( cbNeck, sprite.FrameCount );
      } else
      {
        Log( " --No neck flag found, setting UI to 26." );
        FillComboBox( cbNeck, 26 );
      }

      if( portraitType.CustomFlags.ContainsKey( "cheeks" ) )
      {
        spriteName = (string)portraitType.CustomFlags["cheeks"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Cheeks item count: " + sprite.FrameCount );
        FillComboBox( cbCheeks, sprite.FrameCount );
      } else
      {
        Log( " --No cheeks flag found, setting UI to 26." );
        FillComboBox( cbCheeks, 26 );
      }

      if( portraitType.CustomFlags.ContainsKey( "chin" ) )
      {
        spriteName = (string)portraitType.CustomFlags["chin"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Chin item count: " + sprite.FrameCount );
        FillComboBox( cbChin, sprite.FrameCount );
      } else
      {
        Log( " --No chin flag found, setting UI to 26." );
        FillComboBox( cbChin, 26 );
      }

      if( portraitType.CustomFlags.ContainsKey( "mouth" ) )
      {
        spriteName = (string)portraitType.CustomFlags["mouth"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Mouth item count: " + sprite.FrameCount );
        FillComboBox( cbMouth, sprite.FrameCount );
      } else
      {
        Log( " --No mouth flag found, setting UI to 26." );
        FillComboBox( cbMouth, 26 );
      }

      if( portraitType.CustomFlags.ContainsKey( "nose" ) )
      {
        spriteName = (string)portraitType.CustomFlags["nose"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Nose item count: " + sprite.FrameCount );
        FillComboBox( cbNose, sprite.FrameCount );
      } else
      {
        Log( " --No nose flag found, setting UI to 26." );
        FillComboBox( cbNose, 26 );
      }

      if( portraitType.CustomFlags.ContainsKey( "eyes" ) )
      {
        spriteName = (string)portraitType.CustomFlags["eyes"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Eyes item count: " + sprite.FrameCount );
        FillComboBox( cbEyes, sprite.FrameCount );
      } else
      {
        Log( " --No eyes flag found, setting UI to 26." );
        FillComboBox( cbEyes, 26 );
      }

      if( portraitType.CustomFlags.ContainsKey( "ear" ) )
      {
        spriteName = (string)portraitType.CustomFlags["ear"];
        sprite = portraitReader.Sprites[spriteName];
        Log( " --Ear item count: " + sprite.FrameCount );
        FillComboBox( cbEars, sprite.FrameCount );
      } else
      {
        Log( " --No ear flag found, setting UI to 26." );
        FillComboBox( cbEars, 26 );
      }

      Log( " --Setting hair colours: " + portraitType.HairColours.Count );
      for( int i = 0; i < portraitType.HairColours.Count; i++ )
        cbHairColour.Items.Add( i );
      Log( " --Setting eye colours: " + portraitType.EyeColours.Count );
      for( int i = 0; i < portraitType.EyeColours.Count; i++ )
        cbEyeColour.Items.Add( i );
    }



    private void cb_SelectedIndexChanged( object sender, EventArgs e )
    {
      if( started )
      {
        UpdatePortrait();
      }
    }

    private void btnCopy_Click( object sender, EventArgs e )
    {
      Clipboard.SetText( dnaPropOutput.ToString() );
    }

    private void btnSave_Click( object sender, EventArgs e )
    {
      string file = Measter.Snippets.SaveFileDialog( "Save Image", "PNG|*.png", null );

      if( file != null )
      {
        previewImage.Save( file, ImageFormat.Png );
      }
    }

    private void btnImport_Click( object sender, EventArgs e )
    {
      ImportDialog dialog = new ImportDialog();

      if( dialog.ShowDialog( this ) == DialogResult.OK )
      {
        started = false;

        cbNeck.SelectedIndex = GetIndex( dialog.Neck, cbNeck );
        cbChin.SelectedIndex = GetIndex( dialog.Chin, cbChin );
        cbMouth.SelectedIndex = GetIndex( dialog.Mouth, cbMouth );
        cbNose.SelectedIndex = GetIndex( dialog.Nose, cbNose );
        cbCheeks.SelectedIndex = GetIndex( dialog.Cheeks, cbCheeks );
        cbEyes.SelectedIndex = GetIndex( dialog.Eyes, cbEyes );
        cbEars.SelectedIndex = GetIndex( dialog.Ears, cbEars );
        cbHairColour.SelectedIndex = GetIndex( dialog.HairColour, cbHairColour );
        cbEyeColour.SelectedIndex = GetIndex( dialog.EyeColour, cbEyeColour );

        cbBackground.SelectedIndex = GetIndex( dialog.Background, cbBackground );
        cbHair.SelectedIndex = GetIndex( dialog.Hair, cbHair );
        cbClothes.SelectedIndex = GetIndex( dialog.Clothes, cbClothes );
        cbBeard.SelectedIndex = GetIndex( dialog.Beard, cbBeard );
        cbHeadgear.SelectedIndex = GetIndex( dialog.Headgear, cbHeadgear );
        cbPrisoner.SelectedIndex = GetIndex( dialog.Prison, cbPrisoner );
        cbScars.SelectedIndex = GetIndex( dialog.Scars, cbScars );
        cbRedDots.SelectedIndex = GetIndex( dialog.RedDots, cbRedDots );
        cbBoils.SelectedIndex = GetIndex( dialog.Boils, cbBoils );
        cbBlinded.SelectedIndex = GetIndex( dialog.Blinded, cbBlinded );

        started = true;

        UpdatePortrait();
      }
    }

    private int GetIndex( char letter, ComboBox cb )
    {
      int index = 0;

      if( cb.Items.Count == 0 )
        return -1;

      if( letter == '0' )
        return index;

      index = portraitReader.Letters.IndexOf( letter ) + 1;
      index = index % cb.Items.Count;

      if( index == cb.Items.Count )
        index = 0;

      return index;
    }

    private void btnRandom_Click( object sender, EventArgs e )
    {
      started = false;
      RandomizeUI( false );
      started = true;

      UpdatePortrait();
    }

    private void cbMods_SelectedIndexChanged( object sender, EventArgs e )
    {
      selectedMod = usableMods[cbMods.SelectedIndex];

      if( started )
      {
        SetupStream();

        started = false;
        LoadPortraits();

        if( hadError )
          return;

        SetupSharedUI();
        SetupUI();
        RandomizeUI( false );
        started = true;

        UpdatePortrait();
      }
    }

    private void cbModEnable_CheckedChanged( object sender, EventArgs e )
    {
      cbMods.Enabled = cbModEnable.Checked;

      SetupStream();

      started = false;
      LoadPortraits();

      if( hadError )
        return;

      SetupSharedUI();
      SetupUI();
      RandomizeUI( false );
      started = true;

      UpdatePortrait();
    }

    private void cbPortraitTypes_SelectedIndexChanged( object sender, EventArgs e )
    {
      if( started )
      {
        started = false;
        SetupUI();

        cbNeck.SelectedIndex = GetIndex( dna[0], cbNeck );
        cbChin.SelectedIndex = GetIndex( dna[1], cbChin );
        cbMouth.SelectedIndex = GetIndex( dna[2], cbMouth );
        cbNose.SelectedIndex = GetIndex( dna[3], cbNose );
        cbCheeks.SelectedIndex = GetIndex( dna[4], cbCheeks );
        cbEyes.SelectedIndex = GetIndex( dna[6], cbEyes );
        cbEars.SelectedIndex = GetIndex( dna[7], cbEars );
        cbHairColour.SelectedIndex = GetIndex( dna[8], cbHairColour );
        cbEyeColour.SelectedIndex = GetIndex( dna[9], cbEyeColour );

        cbBackground.SelectedIndex = GetIndex( properties[0], cbBackground );
        cbHair.SelectedIndex = GetIndex( properties[1], cbHair );
        cbClothes.SelectedIndex = GetIndex( properties[3], cbClothes );
        cbBeard.SelectedIndex = GetIndex( properties[4], cbBeard );
        cbHeadgear.SelectedIndex = GetIndex( properties[5], cbHeadgear );
        cbPrisoner.SelectedIndex = GetIndex( properties[6], cbPrisoner );
        cbScars.SelectedIndex = GetIndex( properties[7], cbScars );
        cbRedDots.SelectedIndex = GetIndex( properties[8], cbRedDots );
        cbBoils.SelectedIndex = GetIndex( properties[9], cbBoils );

        started = true;

        UpdatePortrait();
      }
    }

    private void Form1_FormClosed( object sender, FormClosedEventArgs e )
    {
      if( !hadError && forceLog )
        DumpLog();
    }
  }
}
