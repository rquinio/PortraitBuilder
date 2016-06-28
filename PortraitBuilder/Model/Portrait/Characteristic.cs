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

		public Characteristic(string name, int index, Type type) {
			this.name = name;
			this.index = index;
			this.type = type;
		}

		private string name;

		/// <summary>
		/// Index of attribute in dna/properties string
		/// </summary>
		public int index;

		public Type type;

		public enum Type {
			DNA,
			Property
		}

		public override string ToString() {
			char typeCode = (type == Type.DNA) ? 'd' : 'p';
			return string.Format("{0} ({1}{2})", name, typeCode, index);
		}

		public static Characteristic BACKGROUND = new Characteristic("background", 0, Type.Property);
		public static Characteristic HAIR = new Characteristic("hair", 1, Type.Property);
		public static Characteristic HEAD = new Characteristic("head", 2, Type.Property);
		public static Characteristic CLOTHES = new Characteristic("clothes", 3, Type.Property);
		public static Characteristic BEARD = new Characteristic("beard", 4, Type.Property);
		public static Characteristic HEADGEAR = new Characteristic("headgear", 5, Type.Property);
		public static Characteristic IMPRISONED = new Characteristic("imprisoned", 6, Type.Property);
		public static Characteristic SCARS = new Characteristic("scars", 7, Type.Property);
		public static Characteristic RED_DOTS = new Characteristic("reddots", 8, Type.Property);
		public static Characteristic BOILS = new Characteristic("boils", 9, Type.Property);
		public static Characteristic BLINDED = new Characteristic("blinded", 10, Type.Property);
		public static Characteristic PLAYER = new Characteristic("player", 11, Type.Property);

		public static Characteristic NECK = new Characteristic("neck", 0, Type.DNA);
		public static Characteristic CHIN = new Characteristic("chin", 1, Type.DNA);
		public static Characteristic MOUTH = new Characteristic("mouth", 2, Type.DNA);
		public static Characteristic NOSE = new Characteristic("nose", 3, Type.DNA);
		public static Characteristic CHEEKS = new Characteristic("cheeks", 4, Type.DNA);
		public static Characteristic D5 = new Characteristic("d5", 5, Type.DNA);
		public static Characteristic EYES = new Characteristic("eyes", 6, Type.DNA);
		public static Characteristic EARS = new Characteristic("ears", 7, Type.DNA);
		public static Characteristic HAIR_COLOR = new Characteristic("haircolor", 8, Type.DNA);
		public static Characteristic EYE_COLOR = new Characteristic("eyecolor", 9, Type.DNA);
		public static Characteristic D10 = new Characteristic("d10", 10, Type.DNA);

		public static Characteristic[] DNA = new Characteristic[] { NECK, CHIN, MOUTH, NOSE, CHEEKS, D5, EYES, EARS, HAIR_COLOR, EYE_COLOR, D10 };
		public static Characteristic[] PROPERTIES = new Characteristic[] { BACKGROUND, HAIR, HEAD, CLOTHES, BEARD, HEADGEAR, IMPRISONED, SCARS, RED_DOTS, BOILS, BLINDED, PLAYER };
	}
}
