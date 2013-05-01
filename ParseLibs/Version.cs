using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Parsers
{
  static public class Version
  {
    static public string GetVersion()
    {
      Assembly ass = Assembly.GetExecutingAssembly();
      FileVersionInfo fvi = FileVersionInfo.GetVersionInfo( ass.Location );

      return fvi.FileVersion;
    }
  }
}
