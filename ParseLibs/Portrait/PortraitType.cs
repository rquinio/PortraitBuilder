using System.Collections.Generic;

namespace Parsers.Portrait
{
	public class PortraitType
	{
		public string Name;
		public string EffectFile;

		/// <summary>
		/// Letter index of the hair colour in the DNA string
		/// </summary>
		public int HairColourIndex;
		/// <summary>
		/// Letter index of the eye colour in the DNA string
		/// </summary>
		public int EyeColourIndex;

		/// <summary>
		/// Used for storing data specific to the program.
		/// </summary>
		public Dictionary<string, object> CustomFlags = new Dictionary<string, object>();

		public List<Layer> Layers = new List<Layer>();
		public List<Hair> HairColours = new List<Hair>();
		public List<Colour> EyeColours = new List<Colour>();
		public List<int> Culture = new List<int>();

		/// <summary>
		/// The file that the data was loaded from.
		/// </summary>
		public string Filename;

		public override string ToString()
		{
			return string.Format( "Name: {0}, Layers: {1}, HairColours: {2}, EyeColours: {3}", Name, Layers.Count, HairColours.Count, EyeColours.Count );
		}
	}
}