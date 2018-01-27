using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortraitBuilder.Model.Portrait {

	/// <summary>
	/// Represents one DNA or Property element
	/// </summary>
	public class Characteristic {

		public Characteristic(string name, int index, Type type, bool randomizable) {
			this.name = name;
			this.index = index;
			this.type = type;
			this.randomizable = randomizable;
		}

		public Characteristic(string name, int index, Type type, bool randomizable, bool custom)
			: this(name, index, type, randomizable) {
			this.custom = custom;
		}

		private string name;

		/// <summary>
		/// Index of attribute in dna/properties string
		/// </summary>
		public int index;

		public Type type;

		/// <summary>
		/// Whether the characteristic should be randomized when generating a random portrait.
		/// </summary>
		public bool randomizable;

		/// <summary>
		/// Whether the characteristic is a non-vanilla one.
		/// </summary>
		public bool custom = false;

		public enum Type {
			DNA,
			Property
		}

		public override string ToString() {
			char typeCode = (type == Type.DNA) ? 'd' : 'p';
			return string.Format("{0} ({1}{2})", name, typeCode, index);
		}

		public static Characteristic BACKGROUND = new Characteristic("Background", 0, Type.Property, true);
		public static Characteristic HAIR = new Characteristic("Hair", 1, Type.Property, true);
		public static Characteristic HEAD = new Characteristic("Head", 2, Type.Property, true);
		public static Characteristic CLOTHES = new Characteristic("Clothes", 3, Type.Property, true);
		public static Characteristic BEARD = new Characteristic("Beard", 4, Type.Property, true);
		public static Characteristic HEADGEAR = new Characteristic("Headgear", 5, Type.Property, true);
		public static Characteristic IMPRISONED = new Characteristic("Imprisoned", 6, Type.Property, false);
		public static Characteristic SCARS = new Characteristic("Scars", 7, Type.Property, false);
		public static Characteristic RED_DOTS = new Characteristic("Reddots", 8, Type.Property, false);
		public static Characteristic BOILS = new Characteristic("Boils", 9, Type.Property, false);
		public static Characteristic BLINDED = new Characteristic("Blinded", 10, Type.Property, false);
		public static Characteristic PLAYER = new Characteristic("Player", 11, Type.Property, false);
		public static Characteristic MASK = new Characteristic("Mask", 12, Type.Property, false);
		public static Characteristic EYEPATCH = new Characteristic("Eyepatch", 13, Type.Property, false);
		public static Characteristic MAKEUP = new Characteristic("Makeup", 14, Type.Property, false);
		public static Characteristic MAKEUP_2 = new Characteristic("Makeup2", 15, Type.Property, false);
		public static Characteristic JEWELRY = new Characteristic("Jewelry", 16, Type.Property, false);
		public static Characteristic IMMORTALITY = new Characteristic("Immortality", 17, Type.Property, false);

		public static Characteristic NECK = new Characteristic("Neck", 0, Type.DNA, true);
		public static Characteristic CHIN = new Characteristic("Chin", 1, Type.DNA, true);
		public static Characteristic MOUTH = new Characteristic("Mouth", 2, Type.DNA, true);
		public static Characteristic NOSE = new Characteristic("Nose", 3, Type.DNA, true);
		public static Characteristic CHEEKS = new Characteristic("Cheeks", 4, Type.DNA, true);
		public static Characteristic D5 = new Characteristic("Unused", 5, Type.DNA, true);
		public static Characteristic EYES = new Characteristic("Eyes", 6, Type.DNA, true);
		public static Characteristic EARS = new Characteristic("Ears", 7, Type.DNA, true);
		public static Characteristic HAIR_COLOR = new Characteristic("Haircolor", 8, Type.DNA, true);
		public static Characteristic EYE_COLOR = new Characteristic("Eyecolor", 9, Type.DNA, true);
		public static Characteristic D10 = new Characteristic("Unused", 10, Type.DNA, true);

		public static Characteristic[] DNA = new Characteristic[] { NECK, CHIN, MOUTH, NOSE, CHEEKS, D5, EYES, EARS, HAIR_COLOR, EYE_COLOR, D10 };
		public static Characteristic[] PROPERTIES = new Characteristic[] { BACKGROUND, HAIR, HEAD, CLOTHES, BEARD, HEADGEAR, IMPRISONED, SCARS, RED_DOTS, BOILS, BLINDED, PLAYER, MASK, EYEPATCH, MAKEUP, MAKEUP_2, JEWELRY, IMMORTALITY };
	

		public static Characteristic getProperty(int index){
			if (index < PROPERTIES.Length) {
				return PROPERTIES[index];
			} else {
				// As defines are not parsed, always consider custom properties as valid.
				return new Characteristic("Custom", index, Type.Property, false, true);
			}
		}

		public static Characteristic getDna(int index) {
			try { 
				return DNA[index];
			} catch (IndexOutOfRangeException) {
				throw new IndexOutOfRangeException("Characteristic d" + index + " does not exist.");
			}
		}
	}
}
