using System.Collections.Generic;

namespace PortraitBuilder.Model.Portrait {

	/// <summary>
	/// Definition of how a portrait for a given ethnicity, sex, age and date is composed.
	/// </summary>
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
		public List<int> HeadgearThatHidesHair = new List<int>();

		/// <summary>
		/// The file that the data was loaded from (E.g portraits.gfx)
		/// </summary>
		public string Filename;

		public List<Characteristic> getCustomCharacterstics() {
			List<Characteristic> customCharacteristics = new List<Characteristic>();
			foreach (Layer layer in this.Layers) {
				if (layer.Characteristic.custom == true) {
					customCharacteristics.Add(layer.Characteristic);
				}
			}
			return customCharacteristics;
		}

		public override string ToString() {
			return string.Format("Name: {0}, Layers: {1}, HairColours: {2}, EyeColours: {3}", Name, Layers.Count, HairColours.Count, EyeColours.Count);
		}

		/// <summary>
		/// Whether this portraitType is a base portraitType. 
		/// 
		/// Base portraitTypes can also be used as override portraitTypes, with culture-indexed layers replacing the ones of another base.
		/// </summary>
		public bool IsBasePortraitType(){
			return HairColours.Count > 0;
		}

		public Layer GetCultureLayer(int cultureIndex) {
			foreach (Layer layer in this.Layers) {
				if (layer.CultureIndex == cultureIndex) {
					return layer;
				}
			}
			return null;
		}
	}
}