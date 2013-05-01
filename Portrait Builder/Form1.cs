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
    private Image m_previewImage = new Bitmap( 176, 176 );
    private bool m_started = false;
    private StringBuilder m_DNAPropOutput;
    public static Random m_rand = new Random();

    private string m_CKDir = string.Empty;
    private string m_myDocsDir = string.Empty;
    private string m_dlcDir = string.Empty;

    private Mod m_selectedMod = new Mod();
    private List<Mod> m_usableMods = new List<Mod>();
    private PortraitReader m_pReader = new PortraitReader();
    private List<Bitmap> m_borders = new List<Bitmap>();
    private string m_DNA, m_properties;

    private StreamWriter m_log;
    private MemoryStream m_memoryStream;
    private bool m_forceLog = false;
    private bool m_fullLogging = false;
    private string m_fvi = string.Empty;
    private bool m_hadError = false;
    private StringBuilder m_modListSetup;


    public Form1()
    {
      InitializeComponent(); 
      EnvironmentSetup();
    }

    public Form1( bool logging, bool full )
    {
      InitializeComponent();
      
      m_forceLog = logging;
      m_fullLogging = full;   
      
      EnvironmentSetup();
    }

    private void EnvironmentSetup()
    {
      m_myDocsDir = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) +
                    @"\Paradox Interactive\Crusader Kings II\";
      m_dlcDir = Environment.CurrentDirectory + @"\dlc";
      ReadDir();
      m_modListSetup = new StringBuilder();
      LoadDLC();
      LoadMods();
      
      Assembly ass = Assembly.GetExecutingAssembly();
      FileVersionInfo fvi = FileVersionInfo.GetVersionInfo( ass.Location );
      m_fvi = fvi.FileVersion;

      SetupStream();

      LoadPortraits();
      LoadBorders();

      if( m_hadError )
        return;

      SetupSharedUI();
      SetupUI();
      RandomizeUI( true );
      UpdatePortrait();

      m_started = true;
    }

    private void SetupStream()
    {
      m_memoryStream = new MemoryStream();
      m_log = new StreamWriter( m_memoryStream );

      if ( m_fullLogging ) m_pReader.SetLogging( m_log );

      Log( "Portrait Builder Version " + m_fvi );
      Log( "Portrait Builder Parser Library " + Parsers.Version.GetVersion() );
      Log( "Logging started at " + DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" ) );
      Log( "" );

      Log( m_modListSetup.ToString() );
    }

    private void DumpLog()
    {
      FileStream fs = new FileStream( "log.txt", FileMode.Create, FileAccess.Write );
      m_memoryStream.WriteTo( fs );
      fs.Close();
      fs.Dispose();
    }

    private void Log( string text )
    {
      m_log.WriteLine( text );
      m_log.Flush();
    }

    private void LoadBorders()
    {
      Log( "Setting up borders." );
      if( !File.Exists( m_CKDir + @"\gfx\interface\charframe_150.dds" ) )
      {
        Log( "Error: Borders file \\gfx\\interface\\charframe_150.dds not found." );
        m_hadError = true;
        DumpLog();

        MessageBox.Show( this, "Unable to find borders graphic.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
        return;
      }

      Bitmap t = DevIL.DevIL.LoadBitmap( m_CKDir + @"\gfx\interface\charframe_150.dds" );
      Bitmap d;
      Graphics g;

      for( int i = 0; i < 6; i++ )
      {
        d = new Bitmap( 176, 176 );
        g = Graphics.FromImage( d );
        g.DrawImage( t, 0, 0, new Rectangle( i * 176, 0, 176, 176 ), GraphicsUnit.Pixel );
        g.Dispose();
        m_borders.Add( d );
      }
    }

    private void LoadPortraits()
    {
      Log( "Disposing of previous portrait data." );
      m_pReader.Dispose();

      if( cbModEnable.Checked &&
          m_selectedMod.CustomFlags.ContainsKey( "HasPortrait" ) &&
          (bool)m_selectedMod.CustomFlags["HasPortrait"] )
      {
        Log( "Loading portraits from mod: " + m_selectedMod.Name );

        string dir = string.Empty;
        switch ( m_selectedMod.ModPathType )
        {
          case ModReader.Folder.CKDir:
            dir = m_CKDir;
            break;
          case ModReader.Folder.MyDocs:
            dir = m_myDocsDir;
            break;
          case ModReader.Folder.DLC:
            dir = m_dlcDir;
            break;
        }

        string[] fList = Directory.GetFiles( dir + @"\" + m_selectedMod.Path + @"\interface\", "*.gfx" );

        foreach( string s in fList )
          m_pReader.Parse( s );
      } else
      {
        Log( "Loading portraits from vanilla." );

        string[] fList = Directory.GetFiles( m_CKDir + @"\interface\", "*.gfx" );

        foreach( string s in fList )
          m_pReader.Parse( s );
      }

      Log( "The following errors were encountered while loading." );

      foreach( string error in m_pReader.Errors )
        Log( " --" + error );

      cbPortraitTypes.Items.Clear();

      if( m_pReader.PortraitTypes.Count == 0 )
      {
        Log( "Error: No portrait types found." );

        MessageBox.Show( this, "No portraits found in mod " + m_selectedMod.Name + "\n\nCheck errorlog.txt for details.", "Error", MessageBoxButtons.OK,
                         MessageBoxIcon.Error );

        DumpLog();

        m_hadError = true;

        return;
      }
       
      Log( "Setting up type flags" );
      foreach( KeyValuePair<string, PortraitType> pair in m_pReader.PortraitTypes )
      {
        Log( " --Setting up flags for " + pair.Value.Name );
        cbPortraitTypes.Items.Add( pair.Value.Name );

        foreach( Layer l in pair.Value.Layers )
        {
          #region shared
          if( l.Name.Contains( "background" ) && !pair.Value.CustomFlags.ContainsKey( "background" ) )
            pair.Value.CustomFlags.Add( "background", l.Name );

          if( l.Name.Contains( "boils" ) && !pair.Value.CustomFlags.ContainsKey( "boils" ) )
            pair.Value.CustomFlags.Add( "boils", l.Name );

          if( l.Name.Contains( "reddots" ) && !pair.Value.CustomFlags.ContainsKey( "reddots" ) )
            pair.Value.CustomFlags.Add( "reddots", l.Name );

          if( l.Name.Contains( "scars" ) && !pair.Value.CustomFlags.ContainsKey( "scars" ) )
            pair.Value.CustomFlags.Add( "scars", l.Name );

          if( l.Name.Contains( "imprisoned" ) && !pair.Value.CustomFlags.ContainsKey( "imprisoned" ) )
            pair.Value.CustomFlags.Add( "imprisoned", l.Name );

          if( l.Name.Contains( "blinded" ) && !pair.Value.CustomFlags.ContainsKey( "blinded" ) )
            pair.Value.CustomFlags.Add( "blinded", l.Name );
          #endregion

          #region Properties
          if( l.Name.Contains( "clothes" ) && !pair.Value.CustomFlags.ContainsKey( "clothes" ) )
            pair.Value.CustomFlags.Add( "clothes", l.Name );

          if( l.Name.Contains( "headgear" ) && !pair.Value.CustomFlags.ContainsKey( "headgear" ) )
            pair.Value.CustomFlags.Add( "headgear", l.Name );

          if( l.Name.Contains( "beard" ) && !pair.Value.CustomFlags.ContainsKey( "beard" ) )
            pair.Value.CustomFlags.Add( "beard", l.Name );

          if( l.Name.Contains( "hair" ) && !pair.Value.CustomFlags.ContainsKey( "hair" ) )
            pair.Value.CustomFlags.Add( "hair", l.Name );
          #endregion

          #region DNA
          if( l.Name.Contains( "base" ) && !pair.Value.CustomFlags.ContainsKey( "base" ) )
            pair.Value.CustomFlags.Add( "base", l.Name );

          if( l.Name.Contains( "neck" ) && !pair.Value.CustomFlags.ContainsKey( "neck" ) )
            pair.Value.CustomFlags.Add( "neck", l.Name );

          if( l.Name.Contains( "cheeks" ) && !pair.Value.CustomFlags.ContainsKey( "cheeks" ) )
            pair.Value.CustomFlags.Add( "cheeks", l.Name );

          if( l.Name.Contains( "chin" ) && !pair.Value.CustomFlags.ContainsKey( "chin" ) )
            pair.Value.CustomFlags.Add( "chin", l.Name );

          if( l.Name.Contains( "mouth" ) && !pair.Value.CustomFlags.ContainsKey( "mouth" ) )
            pair.Value.CustomFlags.Add( "mouth", l.Name );

          if( l.Name.Contains( "nose" ) && !pair.Value.CustomFlags.ContainsKey( "nose" ) )
            pair.Value.CustomFlags.Add( "nose", l.Name );

          if( l.Name.Contains( "eyes" ) && !pair.Value.CustomFlags.ContainsKey( "eyes" ) )
            pair.Value.CustomFlags.Add( "eyes", l.Name );

          if( l.Name.Contains( "ear" ) && !pair.Value.CustomFlags.ContainsKey( "ear" ) )
            pair.Value.CustomFlags.Add( "ear", l.Name );
          #endregion
        }
      }
      cbPortraitTypes.SelectedIndex = 0;
    }

    private void LoadMods()
    {
      ModReader r = new ModReader();
      m_modListSetup.AppendLine( "Loading mods from " + m_CKDir + @"\mod\" );
      r.ParseFolder( m_CKDir + @"\mod\", ModReader.Folder.CKDir );
      m_modListSetup.AppendLine( "Loading mods from " + m_myDocsDir + @"\mod\" );
      r.ParseFolder( m_myDocsDir + @"\mod\", ModReader.Folder.MyDocs );

      string dir;
      foreach( Mod m in r.Mods )
      {
        dir = m.ModPathType == ModReader.Folder.CKDir ? m_CKDir : m_myDocsDir;

        if( !Directory.Exists( dir + m.Path ) )
          continue;

        if( File.Exists( dir + m.Path + @"\interface\portraits.gfx" ) )
        {
          m.CustomFlags.Add( "HasPortrait", true );
          m_usableMods.Add( m );
          continue;
        }

        if( Directory.Exists( dir + m.Path + @"\gfx\characters\" ) )
        {
          if( Directory.GetDirectories( dir + m.Path + @"\gfx\characters\" ).Length > 0 )
          {
            m.CustomFlags.Add( "HasPortrait", false );
            m_usableMods.Add( m );
          }
        }
      }

      m_modListSetup.AppendLine( " --Usable mods found:" );
      if( m_usableMods.Count > 0 )
      {
        cbModEnable.Enabled = true;
        foreach( Mod m in m_usableMods )
        {
          m_modListSetup.AppendLine( "   --" + m.Name );
          cbMods.Items.Add( m.Name );
          cbMods.SelectedIndex = 0;
        }
      }
    }

    private void LoadDLC()
    {
      DLCReader d = new DLCReader();
      m_modListSetup.AppendLine( "Loading DLC from " + m_CKDir + @"\dlc\" );
      d.ParseFolder( m_CKDir + @"\dlc" );

      FastZip fs = new FastZip();
      foreach ( DLC c in d.DLCs )
        fs.ExtractZip( m_CKDir + c.Archive, m_dlcDir, null );

      if ( !Directory.Exists( m_dlcDir + @"\gfx\characters\" ) )
        return;

      Mod m = new Mod();
      m.ModPathType = ModReader.Folder.DLC;
      m.Name = "DLC Portraits";
      m.Path = string.Empty;
      m.CustomFlags.Add( "HasPortrait", true );
      m_usableMods.Add( m );
    }

    private void ReadDir()
    {
      using( FileStream fs = new FileStream( "gamedir", FileMode.Open ) )
      using( BinaryReader br = new BinaryReader( fs ) )
      {
        m_CKDir = br.ReadString() + @"\";
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

      Graphics g = Graphics.FromImage( m_previewImage );

      Log( "   --Clearing preview." );
      g.Clear( Color.Empty );
      Bitmap t;
      Log( "   --Rendering portrait." );
      try
      {
        if( cbModEnable.Checked )
        {
          t = m_pReader.DrawPortrait( m_CKDir,
                                      m_selectedMod,
                                      m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()],
                                      m_DNA,
                                      m_properties,
                                      m_myDocsDir );
        } else
        {
          t = m_pReader.DrawPortrait( m_CKDir,
                                      m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()],
                                      m_DNA,
                                      m_properties,
                                      m_myDocsDir );
        }
      } catch( Exception e )
      {
        Log( "Error encountered rendering portrait:" );
        Log( e.ToString() );

        MessageBox.Show( this, "Error rendering portrait:\n\n" + e, "Error", MessageBoxButtons.OK,
                         MessageBoxIcon.Error );

        return;
      }
      g.DrawImage( t, 0, 0 );
      Log( "   --Drawing border." );
      g.DrawImage( m_borders[cbRank.SelectedIndex], 0, 0 );

      if ( m_pReader.DrawErrors.Count > 0 )
      {
        Log( "Errors encountered while rendering portrait:" );
        foreach ( string s in m_pReader.DrawErrors )
          Log( " --" + s );
      }

      pbPortrait.Image = m_previewImage;
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

      m_DNA = sb.ToString();

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

      m_properties = sb.ToString();
    }

    private void OutputDNA()
    {
      Log( " --Outputting DNA and Property strings." );
      m_DNAPropOutput = new StringBuilder();

      m_DNAPropOutput.Append( "  dna=\"" );
      m_DNAPropOutput.Append( m_DNA );
      m_DNAPropOutput.AppendLine( "\"" );

      m_DNAPropOutput.Append( "  properties=\"" );
      m_DNAPropOutput.Append( m_properties );
      m_DNAPropOutput.AppendLine( "\"" );
      tbDNA.Text = m_DNAPropOutput.ToString();
    }

    public char GetLetter( ComboBox cb )
    {
      char letter;

      if( cb.SelectedIndex == 0 )
        letter = m_pReader.Letters[cb.Items.Count - 1];
      else if( cb.SelectedIndex == -1 )
        letter = '0';
      else
        letter = m_pReader.Letters[cb.SelectedIndex - 1];

      return letter;
    }

    private void RandomizeUI( bool doRank )
    {
      Log( "Randomizing UI" );
      if( doRank )
        cbRank.SelectedIndex = m_rand.Next( cbRank.Items.Count - 1 );

      cbBackground.SelectedIndex = m_rand.Next( cbBackground.Items.Count - 1 );
      cbScars.SelectedIndex = m_rand.Next( cbScars.Items.Count - 1 );
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
      cbClothes.SelectedIndex = m_rand.Next( cbClothes.Items.Count - 1 );
      cbHeadgear.SelectedIndex = m_rand.Next( cbHeadgear.Items.Count - 1 );
      cbHair.SelectedIndex = m_rand.Next( cbHair.Items.Count - 1 );
      cbBeard.SelectedIndex = m_rand.Next( cbBeard.Items.Count - 1 );
      cbNeck.SelectedIndex = m_rand.Next( cbNeck.Items.Count - 1 );
      cbCheeks.SelectedIndex = m_rand.Next( cbCheeks.Items.Count - 1 );
      cbChin.SelectedIndex = m_rand.Next( cbChin.Items.Count - 1 );
      cbMouth.SelectedIndex = m_rand.Next( cbMouth.Items.Count - 1 );
      cbNose.SelectedIndex = m_rand.Next( cbNose.Items.Count - 1 );
      cbEyes.SelectedIndex = m_rand.Next( cbEyes.Items.Count - 1 );
      cbEars.SelectedIndex = m_rand.Next( cbEars.Items.Count - 1 );
      cbHairColour.SelectedIndex = m_rand.Next( cbHairColour.Items.Count - 1 );
      cbEyeColour.SelectedIndex = m_rand.Next( cbEyeColour.Items.Count - 1 );
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

      string sname;
      Sprite t;

      if( m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags.ContainsKey( "background" ) )
      {
        sname = (string)m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags["background"];
        t = m_pReader.Sprites[sname];
        Log( " --Background item count: " + t.FrameCount );
        FillComboBox( cbBackground, t.FrameCount );
      } else
      {
        Log( " --No background flag found, setting UI to 26." );
        FillComboBox( cbBackground, 26 );
      }

      if( m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags.ContainsKey( "scars" ) )
      {
        sname = (string)m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags["scars"];
        t = m_pReader.Sprites[sname];
        Log( " --Scars item count: " + t.FrameCount );
        FillComboBox( cbScars, t.FrameCount );
      } else
      {
        Log( " --No scars flag found, setting UI to 26." );
        FillComboBox( cbScars, 26 );
      }

      if( m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags.ContainsKey( "reddots" ) )
      {
        sname = (string)m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags["reddots"];
        t = m_pReader.Sprites[sname];
        Log( " --Reddots item count: " + t.FrameCount );
        FillComboBox( cbRedDots, t.FrameCount );
      } else
      {
        Log( " --No reddots flag found, setting UI to 26." );
        FillComboBox( cbRedDots, 26 );
      }

      if( m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags.ContainsKey( "boils" ) )
      {
        sname = (string)m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags["boils"];
        t = m_pReader.Sprites[sname];
        Log( " --Boils item count: " + t.FrameCount );
        FillComboBox( cbBoils, t.FrameCount );
      } else
      {
        Log( " --No boils flag found, setting UI to 26." );
        FillComboBox( cbBoils, 26 );
      }

      if( m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags.ContainsKey( "imprisoned" ) )
      {
        sname = (string)m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags["imprisoned"];
        t = m_pReader.Sprites[sname];
        Log( " --Imprisoned item count: " + t.FrameCount );
        FillComboBox( cbPrisoner, t.FrameCount );
      } else
      {
        Log( " --No imprisoned flag found, setting UI to 26." );
        FillComboBox( cbPrisoner, 26 );
      }

      if( m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags.ContainsKey( "blinded" ) )
      {
        sname = (string)m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()].CustomFlags["blinded"];
        t = m_pReader.Sprites[sname];
        Log( " --Blinded item count: " + t.FrameCount );
        FillComboBox( cbBlinded, t.FrameCount );
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

      Sprite t;
      string sname;
      PortraitType p = m_pReader.PortraitTypes[cbPortraitTypes.SelectedItem.ToString()];

      Log( "Setting up UI for: " + p.Name );

      if( p.CustomFlags.ContainsKey( "clothes" ) )
      {
        sname = (string)p.CustomFlags["clothes"];
        t = m_pReader.Sprites[sname];
        Log( " --Clothes item count: " + t.FrameCount );
        FillComboBox( cbClothes, t.FrameCount );
      } else
      {
        Log( " --No clothes flag found, setting UI to 26." );
        FillComboBox( cbClothes, 26 );
      }

      if( p.CustomFlags.ContainsKey( "headgear" ) )
      {
        sname = (string)p.CustomFlags["headgear"];
        t = m_pReader.Sprites[sname];
        Log( " --Headgear item count: " + t.FrameCount );
        FillComboBox( cbHeadgear, t.FrameCount );
      } else
      {
        Log( " --No headgear flag found, setting UI to 26." );
        FillComboBox( cbHeadgear, 26 );
      }

      if( p.CustomFlags.ContainsKey( "hair" ) )
      {
        sname = (string)p.CustomFlags["hair"];
        t = m_pReader.Sprites[sname];
        Log( " --Hair item count: " + t.FrameCount );
        FillComboBox( cbHair, t.FrameCount );
      } else
      {
        Log( " --No hair flag found, setting UI to 26." );
        FillComboBox( cbHair, 26 );
      }

      if( p.CustomFlags.ContainsKey( "beard" ) )
      {
        sname = (string)p.CustomFlags["beard"];
        t = m_pReader.Sprites[sname];
        Log( " --Beard item count: " + t.FrameCount );
        FillComboBox( cbBeard, t.FrameCount );
      } else
      {
        Log( " --No beard flag found, setting UI to 26." );
        FillComboBox( cbBeard, 26 );
      }

      if( p.CustomFlags.ContainsKey( "neck" ) )
      {
        sname = (string)p.CustomFlags["neck"];
        t = m_pReader.Sprites[sname];
        Log( " --Neck item count: " + t.FrameCount );
        FillComboBox( cbNeck, t.FrameCount );
      } else
      {
        Log( " --No neck flag found, setting UI to 26." );
        FillComboBox( cbNeck, 26 );
      }

      if( p.CustomFlags.ContainsKey( "cheeks" ) )
      {
        sname = (string)p.CustomFlags["cheeks"];
        t = m_pReader.Sprites[sname];
        Log( " --Cheeks item count: " + t.FrameCount );
        FillComboBox( cbCheeks, t.FrameCount );
      } else
      {
        Log( " --No cheeks flag found, setting UI to 26." );
        FillComboBox( cbCheeks, 26 );
      }

      if( p.CustomFlags.ContainsKey( "chin" ) )
      {
        sname = (string)p.CustomFlags["chin"];
        t = m_pReader.Sprites[sname];
        Log( " --Chin item count: " + t.FrameCount );
        FillComboBox( cbChin, t.FrameCount );
      } else
      {
        Log( " --No chin flag found, setting UI to 26." );
        FillComboBox( cbChin, 26 );
      }

      if( p.CustomFlags.ContainsKey( "mouth" ) )
      {
        sname = (string)p.CustomFlags["mouth"];
        t = m_pReader.Sprites[sname];
        Log( " --Mouth item count: " + t.FrameCount );
        FillComboBox( cbMouth, t.FrameCount );
      } else
      {
        Log( " --No mouth flag found, setting UI to 26." );
        FillComboBox( cbMouth, 26 );
      }

      if( p.CustomFlags.ContainsKey( "nose" ) )
      {
        sname = (string)p.CustomFlags["nose"];
        t = m_pReader.Sprites[sname];
        Log( " --Nose item count: " + t.FrameCount );
        FillComboBox( cbNose, t.FrameCount );
      } else
      {
        Log( " --No nose flag found, setting UI to 26." );
        FillComboBox( cbNose, 26 );
      }

      if( p.CustomFlags.ContainsKey( "eyes" ) )
      {
        sname = (string)p.CustomFlags["eyes"];
        t = m_pReader.Sprites[sname];
        Log( " --Eyes item count: " + t.FrameCount );
        FillComboBox( cbEyes, t.FrameCount );
      } else
      {
        Log( " --No eyes flag found, setting UI to 26." );
        FillComboBox( cbEyes, 26 );
      }

      if( p.CustomFlags.ContainsKey( "ear" ) )
      {
        sname = (string)p.CustomFlags["ear"];
        t = m_pReader.Sprites[sname];
        Log( " --Ear item count: " + t.FrameCount );
        FillComboBox( cbEars, t.FrameCount );
      } else
      {
        Log( " --No ear flag found, setting UI to 26." );
        FillComboBox( cbEars, 26 );
      }

      Log( " --Setting hair colours: " + p.HairColours.Count );
      for( int i = 0; i < p.HairColours.Count; i++ )
        cbHairColour.Items.Add( i );
      Log( " --Setting eye colours: " + p.EyeColours.Count );
      for( int i = 0; i < p.EyeColours.Count; i++ )
        cbEyeColour.Items.Add( i );
    }



    private void cb_SelectedIndexChanged( object sender, EventArgs e )
    {
      if( m_started )
      {
        UpdatePortrait();
      }
    }

    private void btnCopy_Click( object sender, EventArgs e )
    {
      Clipboard.SetText( m_DNAPropOutput.ToString() );
    }

    private void btnSave_Click( object sender, EventArgs e )
    {
      string file = Measter.Snippets.SaveFileDialog( "Save Image", "PNG|*.png", null );

      if( file != null )
      {
        m_previewImage.Save( file, ImageFormat.Png );
      }
    }

    private void btnImport_Click( object sender, EventArgs e )
    {
      ImportDialog d = new ImportDialog();

      if( d.ShowDialog( this ) == DialogResult.OK )
      {
        m_started = false;

        cbNeck.SelectedIndex = GetIndex( d.Neck, cbNeck );
        cbChin.SelectedIndex = GetIndex( d.Chin, cbChin );
        cbMouth.SelectedIndex = GetIndex( d.Mouth, cbMouth );
        cbNose.SelectedIndex = GetIndex( d.Nose, cbNose );
        cbCheeks.SelectedIndex = GetIndex( d.Cheeks, cbCheeks );
        cbEyes.SelectedIndex = GetIndex( d.Eyes, cbEyes );
        cbEars.SelectedIndex = GetIndex( d.Ears, cbEars );
        cbHairColour.SelectedIndex = GetIndex( d.HairColour, cbHairColour );
        cbEyeColour.SelectedIndex = GetIndex( d.EyeColour, cbEyeColour );

        cbBackground.SelectedIndex = GetIndex( d.Background, cbBackground );
        cbHair.SelectedIndex = GetIndex( d.Hair, cbHair );
        cbClothes.SelectedIndex = GetIndex( d.Clothes, cbClothes );
        cbBeard.SelectedIndex = GetIndex( d.Beard, cbBeard );
        cbHeadgear.SelectedIndex = GetIndex( d.Headgear, cbHeadgear );
        cbPrisoner.SelectedIndex = GetIndex( d.Prison, cbPrisoner );
        cbScars.SelectedIndex = GetIndex( d.Scars, cbScars );
        cbRedDots.SelectedIndex = GetIndex( d.RedDots, cbRedDots );
        cbBoils.SelectedIndex = GetIndex( d.Boils, cbBoils );
        cbBlinded.SelectedIndex = GetIndex( d.Blinded, cbBlinded );

        m_started = true;

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

      index = m_pReader.Letters.IndexOf( letter ) + 1;
      index = index % cb.Items.Count;

      if( index == cb.Items.Count )
        index = 0;

      return index;
    }

    private void btnRandom_Click( object sender, EventArgs e )
    {
      m_started = false;
      RandomizeUI( false );
      m_started = true;

      UpdatePortrait();
    }

    private void cbMods_SelectedIndexChanged( object sender, EventArgs e )
    {
      m_selectedMod = m_usableMods[cbMods.SelectedIndex];

      if( m_started )
      {
        SetupStream();

        m_started = false;
        LoadPortraits();

        if( m_hadError )
          return;

        SetupSharedUI();
        SetupUI();
        RandomizeUI( false );
        m_started = true;

        UpdatePortrait();
      }
    }

    private void cbModEnable_CheckedChanged( object sender, EventArgs e )
    {
      cbMods.Enabled = cbModEnable.Checked;

      SetupStream();

      m_started = false;
      LoadPortraits();

      if( m_hadError )
        return;

      SetupSharedUI();
      SetupUI();
      RandomizeUI( false );
      m_started = true;

      UpdatePortrait();
    }

    private void cbPortraitTypes_SelectedIndexChanged( object sender, EventArgs e )
    {
      if( m_started )
      {
        m_started = false;
        SetupUI();

        cbNeck.SelectedIndex = GetIndex( m_DNA[0], cbNeck );
        cbChin.SelectedIndex = GetIndex( m_DNA[1], cbChin );
        cbMouth.SelectedIndex = GetIndex( m_DNA[2], cbMouth );
        cbNose.SelectedIndex = GetIndex( m_DNA[3], cbNose );
        cbCheeks.SelectedIndex = GetIndex( m_DNA[4], cbCheeks );
        cbEyes.SelectedIndex = GetIndex( m_DNA[6], cbEyes );
        cbEars.SelectedIndex = GetIndex( m_DNA[7], cbEars );
        cbHairColour.SelectedIndex = GetIndex( m_DNA[8], cbHairColour );
        cbEyeColour.SelectedIndex = GetIndex( m_DNA[9], cbEyeColour );

        cbBackground.SelectedIndex = GetIndex( m_properties[0], cbBackground );
        cbHair.SelectedIndex = GetIndex( m_properties[1], cbHair );
        cbClothes.SelectedIndex = GetIndex( m_properties[3], cbClothes );
        cbBeard.SelectedIndex = GetIndex( m_properties[4], cbBeard );
        cbHeadgear.SelectedIndex = GetIndex( m_properties[5], cbHeadgear );
        cbPrisoner.SelectedIndex = GetIndex( m_properties[6], cbPrisoner );
        cbScars.SelectedIndex = GetIndex( m_properties[7], cbScars );
        cbRedDots.SelectedIndex = GetIndex( m_properties[8], cbRedDots );
        cbBoils.SelectedIndex = GetIndex( m_properties[9], cbBoils );

        m_started = true;

        UpdatePortrait();
      }
    }

    private void Form1_FormClosed( object sender, FormClosedEventArgs e )
    {
      if( !m_hadError && m_forceLog )
        DumpLog();
    }
  }
}
