using System.Collections.Generic;

namespace Parsers.Portrait {
	public class PortraitType {

		/// <summary>
		/// E.g. PORTRAIT_westerngfx_male
		/// </summary>
		public string Name;

		public string EffectFile;

		/// <summary>
		/// Letter index of the hair colour in the DNA string
		/// </summary>
		public int HairColourIndex = Characteristic.HAIR_COLOR.index;

		/// <summary>
		/// Letter index of the eye colour in the DNA string
		/// </summary>
		public int EyeColourIndex = Characteristic.EYE_COLOR.index;

		/// <summary>
		/// List of layers composing the portraitType definition
		/// </summary>
		public List<Layer> Layers = new List<Layer>();

		public List<Hair> HairColours = new List<Hair>();
		public List<Colour> EyeColours = new List<Colour>();

		/// <summary>
		/// TODO How is this used in-game ?
		/// </summary>
		public List<int> Culture = new List<int>();

		/// <summary>
		/// The file that the data was loaded from (E.g portraits.gfx)
		/// </summary>
		public string Filename;

		public override string ToString() {
			return string.Format("Name: {0}, Layers: {1}, HairColours: {2}, EyeColours: {3}", Name, Layers.Count, HairColours.Count, EyeColours.Count);
		}
	}
}