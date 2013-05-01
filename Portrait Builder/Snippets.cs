using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Measter
{
  static class Snippets
  {
    public static string FolderBrowser( string sDescription, string sReturn)
    {
      using ( FolderBrowserDialog dFolder = new FolderBrowserDialog() )
      {
        dFolder.Description = sDescription;
        dFolder.ShowNewFolderButton = false;

        if ( dFolder.ShowDialog() == DialogResult.OK )
        {
          return dFolder.SelectedPath;
        } else
        {
          return sReturn;
        }
      }
    }
    
    public static string OpenFileDialog( string sTitle, string sFilter, string sReturn )
    {
      using( OpenFileDialog dFileDialog = new OpenFileDialog() )
      {
        dFileDialog.Title = sTitle;
        dFileDialog.CheckFileExists = true;
        dFileDialog.CheckPathExists = true;
        dFileDialog.Filter = sFilter;

        if( dFileDialog.ShowDialog() == DialogResult.OK )
        {
          return dFileDialog.FileName;
        } else
        {
          return sReturn;
        }
      }
    }

    public static string SaveFileDialog( string sTitle, string sFilter, string sReturn )
    {
      using( SaveFileDialog dFileDialog = new SaveFileDialog() )
      {
        dFileDialog.Title = sTitle;
        dFileDialog.CheckPathExists = true;
        dFileDialog.Filter = sFilter;

        if( dFileDialog.ShowDialog() == DialogResult.OK )
        {
          return dFileDialog.FileName;
        } else
        {
          return sReturn;
        }
      }
    }
  }
}
