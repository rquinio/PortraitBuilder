using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Parsers.Mod
{
  public class ModReader
  {
    /// <summary>
    /// List of loaded Mods
    /// </summary>
    public List<Mod> Mods = new List<Mod>();

    /// <summary>
    /// List of errors encountered during parsing.
    /// </summary>
    public List<string> Errors = new List<string>();

    public void Parse( string filename, Folder dir )
    {
      if( !File.Exists( filename ) )
      {
        Errors.Add( string.Format( "File not found: {0}", filename ) );
        return;
      }
      
      Mod m;
      string line;
      FileInfo f = new FileInfo( filename );
      m = new Mod();
      m.ModFile = f.Name;
      m.ModPathType = dir;

      using( StreamReader sr = new StreamReader( filename, Encoding.GetEncoding( 1252 ) ) )
      {
        while( ( line = sr.ReadLine() ) != null )
        {
          if( line.StartsWith( "#" ) )
            continue;

          if( line.StartsWith( "name" ) )
            m.Name = line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim();
          if( line.StartsWith( "path" ) || line.StartsWith( "archive" ) )
            m.Path = line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim();
          if( line.StartsWith( "user_dir" ) || line.StartsWith( "archive" ) )
            m.UserDir = line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim();

          if( line.StartsWith( "extend" ) )
            m.Extends.Add( line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim() );
          if( line.StartsWith( "replace" ) )
            m.Replaces.Add( line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim().Replace( '/', '\\' ) );

          if( line.StartsWith( "dependencies" ) )
          {
            string[] tstring = line.Split( '=' )[1].Split( '#' )[0].Replace( "{", "" ).Replace( "}", "" ).Trim().Split( '"' );
            foreach ( string s in tstring )
            {
              if ( s.Trim() != string.Empty )
              {
                m.Dependencies.Add( s );
              }
            }
          }
        }
      }
      Mods.Add( m );
    }

    /// <summary>
    /// Loads all files in the given folder.
    /// </summary>
    /// <param name="folder">Path to the folder containing the files to load.</param>
    /// <param name="dirType">Specifies whether path is CKII install or My Docs.</param>
    public void ParseFolder( string folder, Folder dirType )
    {
      DirectoryInfo dir = new DirectoryInfo( folder );

      if ( !dir.Exists )
      {
        Errors.Add( string.Format( "Folder not found: {0}", dir.FullName ) );
        return;
      }
      
      FileInfo[] mods = dir.GetFiles( "*.mod" );

      if ( mods.Length == 0 )
      {
        Errors.Add( string.Format( "No mods found in folder: {0}", dir.FullName ) );
        return;
      }

      foreach( FileInfo f in mods )
      {
        Parse( f.FullName, dirType );
      }
    }

    public enum Folder {
      CKDir,
      MyDocs,
      DLC
    }
  }
}
