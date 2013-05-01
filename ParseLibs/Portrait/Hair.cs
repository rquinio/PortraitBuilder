using System.Collections.Generic;

namespace Parsers.Portrait
{
  public class Hair
  {
    public Colour Dark;
    public Colour Base;
    public Colour Highlight;

    /// <summary>
    /// Used for storing data specific to the program.
    /// </summary>
    public Dictionary<string, object> CustomFlags = new Dictionary<string, object>();
  }
}