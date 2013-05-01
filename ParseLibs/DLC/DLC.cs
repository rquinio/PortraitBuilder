using System.Collections.Generic;

namespace Parsers.DLC
{
  public class DLC
  {
    public string DLCFile;
    public string Name;
    public string Archive;
    public string Checksum;

    public int SteamID;
    public int GamersGateID;

    public bool AffectsChecksum;

    /// <summary>
    /// Used for storing data specific to the program.
    /// </summary>
    public Dictionary<string, object> CustomFlags = new Dictionary<string, object>();

    public override string ToString()
    {
      return string.Format( "Name: {0}, Archive: {1}", Name, Archive );
    }
  }
}
