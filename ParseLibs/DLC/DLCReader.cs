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

      DLC dlc;
      string line;
      int intOut;
      FileInfo dlcFile = new FileInfo( filename );

      dlc = new DLC();
      dlc.DLCFile = dlcFile.Name;

      using( StreamReader reader = new StreamReader( filename, Encoding.GetEncoding( 1252 ) ) )
      {
        while( ( line = reader.ReadLine() ) != null )
        {
          if( line.StartsWith( "#" ) )
            continue;

          if( line.StartsWith( "name" ) )
            dlc.Name = line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim();
          if( line.StartsWith( "archive" ) )
            dlc.Archive = line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim();
          if( line.StartsWith( "checksum" ) )
            dlc.Checksum = line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim();

          if( line.StartsWith( "steam_id" ) )
          {
            if( Int32.TryParse( line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim(), out intOut ) )
              dlc.SteamID = intOut;
            else
              Errors.Add( string.Format( "Error parsing Steam ID in file: {0}", dlcFile.Name ) );
          }

          if( line.StartsWith( "gamersgate_id" ) )
          {
            if( Int32.TryParse( line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim(), out intOut ) )
              dlc.GamersGateID = intOut;
            else
              Errors.Add( string.Format( "Error parsing GamersGate ID in file: {0}", dlcFile.Name ) );
          }

          if ( line.StartsWith( "affects_checksum" ) )
            dlc.AffectsChecksum = line.Split( '=' )[1].Split( '#' )[0].Replace( "\"", "" ).Trim() == "yes";
        }
      }
      DLCs.Add( dlc );
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

      foreach ( FileInfo dlcFile in dlcFiles )
      {
        Parse( dlcFile.FullName );
      }
    }
  }
}
