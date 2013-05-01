using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Portrait_Builder
{
  public partial class ImportDialog : Form
  {
    private bool m_DNAValid = false, m_PropertiesValid = false;

    public char Neck = 'a';
    public char Chin = 'a';
    public char Mouth = 'a';
    public char Nose = 'a';
    public char Cheeks = 'a';
    public char Eyes = 'a';
    public char Ears = 'a';
    public char HairColour = 'a';
    public char EyeColour = 'a';

    public char Background = 'a';
    public char Hair = 'a';
    public char prop3 = 'a';
    public char Clothes = 'a';
    public char Beard = 'a';
    public char Headgear = 'a';
    public char Prison = 'a';
    public char Scars = 'a';
    public char RedDots = 'a';
    public char Boils = 'a';
    public char Blinded = 'a';
    
    public ImportDialog()
    {
      InitializeComponent();
    }

    private void btnCancel_Click( object sender, EventArgs e )
    {
      Close();
    }

    private void btnOK_Click( object sender, EventArgs e )
    {
      //Get DNA
      string dna = tbDNA.Text;
      Neck = dna[0];
      Chin = dna[1];
      Mouth = dna[2];
      Nose = dna[3];
      Cheeks = dna[4];
      //unused
      Eyes = dna[6];
      Ears = dna[7];
      HairColour = dna[8];
      EyeColour = dna[9];

      //Properties
      string prop = tbProperties.Text;
      Background = prop[0];
      Hair = prop[1];
      //unused
      Clothes = prop[3];
      Beard = prop[4];
      Headgear = prop[5];
      Prison = prop[6];
      Scars = prop[7];
      RedDots = prop[8];
      Boils = prop[9];
      Blinded = prop[10];
      
      DialogResult = DialogResult.OK;
      Close();
    }

    private void tb_TextChanged( object sender, EventArgs e )
    {
      TextBox s = (TextBox)sender;

      bool valid = IsValid( s.Text, 11 );

      if ( s == tbDNA && !valid )
      {
        errorProvider.SetError( tbDNA, "Invalid DNA Code." );
        m_DNAValid = false;
      } else if ( s == tbDNA && valid )
      {
        errorProvider.SetError( tbDNA, string.Empty );
        m_DNAValid = true;
      }

      if( s == tbProperties && !valid )
      {
        errorProvider.SetError( tbProperties, "Invalid Properties Code." );
        m_PropertiesValid = false;
      } else if( s == tbProperties && valid )
      {
        errorProvider.SetError( tbProperties, string.Empty );
        m_PropertiesValid = true;
      }

      if ( m_DNAValid && m_PropertiesValid )
      {
        btnOK.Enabled = true;
      } else
      {
        btnOK.Enabled = false;
      }
    }

    private bool IsValid( string s, int length )
    {
      bool valid = true;

      foreach ( char c in s )
      {
        if( !Char.IsLetterOrDigit( c ) )
        {
          valid = false;
          break;
        }
      }

      if ( s.Length != length )
        valid = false;

      return valid;
    }
  }
}
