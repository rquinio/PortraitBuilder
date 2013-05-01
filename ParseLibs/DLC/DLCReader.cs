using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Parsers.DLC
{
  public class DLCReader
  {
    /// <summary>
    /// List of loaded DLCs.
    /// </summary>
    public List<DLC> DLCs = new List<DLC>();

    /// <summary>
    /// List of errors encountered during parsing.
    /// </summary>
    public List<string> Errors = new List<string>();

    public void Parse( string filename )
    {
      if( !File.Exists( filename ) )
      {
        Errors.Add( string.Format( "File not found: {0}", filename ) );
        return;
      }

      DLC d;
      string line;
      int intOut;
      FileInfo f = new FileInfo( filename );

      d = new DLC();
      d.DLCFile = f.Name;

      using( StreamReader sr = new StreamReader( filename, Encoding.GetEncoding( 1252 ) ) )
      {
        while( ( line = sr.ReadLine() ) != null )
        {
          if( line.StartsWith( "#" ) )
            continue;

          if( line.StartsWith( "name" ) )
            d.Name = line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim();
          if( line.StartsWith( "archive" ) )
            d.Archive = line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim();
          if( line.StartsWith( "checksum" ) )
            d.Checksum = line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim();

          if( line.StartsWith( "steam_id" ) )
          {
            if( Int32.TryParse( line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim(), out intOut ) )
              d.SteamID = intOut;
            else
              Errors.Add( string.Format( "Error parsing Steam ID in file: {0}", f.Name ) );
          }

          if( line.StartsWith( "gamersgate_id" ) )
          {
            if( Int32.TryParse( line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim(), out intOut ) )
              d.GamersGateID = intOut;
            else
              Errors.Add( string.Format( "Error parsing GamersGate ID in file: {0}", f.Name ) );
          }

          if ( line.StartsWith( "affects_checksum" ) )
            d.AffectsChecksum = line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim() == "yes";
        }
      }
      DLCs.Add( d );
    }

    public void ParseFolder( string folder )
    {
      DirectoryInfo dir = new DirectoryInfo( folder );
      if ( !dir.Exists )
      {
        Errors.Add( string.Format( "Folder not found: {0}", dir.FullName ) );
        return;
      }

      FileInfo[] dlcFiles = dir.GetFiles( "*.dlc" );
      if ( dlcFiles.Length == 0 )
      {
        Errors.Add( string.Format( "No DLC files found in folder: {0}", dir.FullName ) );
        return;
      }

      foreach ( FileInfo f in dlcFiles )
      {
        Parse( f.FullName );
      }
    }
  }
}
