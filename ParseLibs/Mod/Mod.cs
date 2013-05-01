using System.Collections.Generic;

namespace Parsers.Mod
{
  public class Mod
  {
    public string Name;
    public string ModFile;
    public string Path;
    public string UserDir;
    public ModReader.Folder ModPathType;
    public List<string> Dependencies = new List<string>();
    public List<string> Extends = new List<string>();
    public List<string> Replaces = new List<string>();

    /// <summary>
    /// Used for storing data specific to the program.
    /// </summary>
    public Dictionary<string, object> CustomFlags = new Dictionary<string, object>();

    public override string ToString()
    {
      return Name;
    }
  }
}