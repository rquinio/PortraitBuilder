using System.Collections.Generic;
using System.Drawing;

namespace Parsers.Portrait
{
	/// <summary>
	/// Used for drawing in portraits because it's faster than the .net Color object.
	/// </summary>
	public class Colour
	{
		public byte Red;
		public byte Green;
		public byte Blue;
		public byte Alpha;

		/// <summary>
		/// Used for storing data specific to the program.
		/// </summary>
		public Dictionary<string, object> CustomFlags = new Dictionary<string, object>();

		/// <summary>
		/// Returns a standard .net Color object with the same values.
		/// </summary>
		public Color ToColor()
		{
			return Color.FromArgb( Alpha, Red, Green, Blue );
		}

		/// <summary>
		/// Returns a Colour object from a standard .net Color object.
		/// </summary>
		public static Colour FromColor( Color cin )
		{
			Colour cout = new Colour();

			cout.Red = cin.R;
			cout.Green = cin.G;
			cout.Blue = cin.B;

			return cout;
		}

		/// <summary>
		/// Linear interpolation function.
		/// </summary>
		/// <param name="l">Left hand colour.</param>
		/// <param name="r">Right hand colour.</param>
		/// <param name="inter">Interval of the interpolation. Uses a range of 0-255</param>
		/// <returns>Returns an interpolation colour.</returns>
		public static Colour Lerp( Colour l, Colour r, double inter )
		{
			Colour c = new Colour();

			c.Red = (byte)( (int)( l.Red ) + ( ( inter / 255f ) * ( (int)( r.Red ) - (int)( l.Red ) ) ) );
			c.Green = (byte)( (int)( l.Green ) + ( ( inter / 255f ) * ( (int)( r.Green ) - (int)( l.Green ) ) ) );
			c.Blue = (byte)( (int)( l.Blue ) + ( ( inter / 255f ) * ( (int)( r.Blue ) - (int)( l.Blue ) ) ) );

			return c;
		}

		/// <summary>
		/// Clamps an input value to a range of 0-255
		/// </summary>
		public static double Clamp( double i )
		{
			if( i > 255 )
				return 255;
			if( i < 0 )
				return 0;
			return i;
		}

		public override string ToString()
		{
			return string.Format( "Red: {0}, Green: {1}, Blue: {2}, Alpha: {3}", Red, Green, Blue, Alpha );
		}
	}
}