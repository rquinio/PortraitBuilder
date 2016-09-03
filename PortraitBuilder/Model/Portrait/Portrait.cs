using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortraitBuilder.Model.Portrait {

	/// <summary>
	/// Input data (dna and properties) used to display a portrait.
	/// 
	/// TODO Rename this to Character !
	/// </summary>
	public class Portrait {

		public static List<char> Letters = new List<char> { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

		private static Random rand = new Random();

		private string dna = "aaaaa0aaaa0";

		private string properties = "aa0aaaaaaaa000";

		private PortraitType portraitType;

		/// <summary>
		/// Index of rank in border sprite
		/// </summary>
		private int rank = 0;

		/// <summary>
		/// Index of government in PortraitRenderer#governmentSpriteSuffix
		/// 
		/// It is hardcoded to vanilla governments only.
		/// </summary>
		private int government = 0;

		private Sex sex = Sex.MALE;

		private string ethnicity;

		private int age;

		private string date;

		public enum Sex {
			MALE,
			FEMALE
		}

		public void import(string dna, string properties) {
			if (dna.Length < 10 || properties.Length < 11) {
				throw new ArgumentException(string.Format("DNA {0} or Property {1} strings are too short.", dna, properties));
			}

			this.dna = dna;
			this.properties = properties;
		}

		public string GetDNA() {
			return dna;
		}

		public string GetProperties() {
			return properties;
		}

		public int GetRank() {
			return rank;
		}

		public void SetRank(int rank) {
			this.rank = rank;
		}

		public int GetGovernment() {
			return government;
		}

		public void SetGovernment(int government) {
			this.government = government;
		}

		public bool IsReligious() {
			return government == 2;
		}

		public bool IsMerchant() {
			return government == 4;
		}

		public Sex GetSex() {
			return sex;
		}

		public void SetPortraitType(PortraitType portraitType) {
			this.portraitType = portraitType;
			if (portraitType.Name.Contains("male")) {
				this.sex = Sex.MALE;
			} else if (portraitType.Name.Contains("female")) {
				this.sex = Sex.FEMALE;
			}
		}

		public PortraitType GetPortraitType() {
			return portraitType;
		}

		public char GetLetter(Characteristic characteristic) {
			char letter;
			if(characteristic == Characteristic.HAIR_COLOR) {
				letter = GetDNA()[portraitType.HairColourIndex];
			} else if(characteristic == Characteristic.EYE_COLOR) {
				letter = GetDNA()[portraitType.EyeColourIndex];
			} else if (characteristic.type == Characteristic.Type.DNA) {
				letter = GetDNA()[characteristic.index];
			} else {
				letter = GetProperties()[characteristic.index]; ;
			}
			return letter;
		}


		public override string ToString() {

			return string.Format("DNA: {0}, Properties: {1}", GetDNA(), GetProperties());
		}

		/// <summary>
		/// Converts a letter to an index
		/// 
		/// In case "letter" is 0, the index is 0
		/// </summary>
		/// <param name="letter"></param>
		/// <param name="total"></param>
		/// <returns></returns>
		public static int GetIndex(char letter, int total) {
			if (total == 0)
				return -1;

			if (letter == '0')
				return 0;

			int index = (Letters.IndexOf(letter) + 1) % total;
			if (index == total) {
				index = 0;
			}
			return index;
		}

		/// <summary>
		/// Converts an index to a letter
		/// 
		/// In case index is 0, the "letter" is 0, rather than Letters[(total - 1)%26]
		/// This is to match how vanilla deals with characteristics in character history
		/// 
		/// Note: in some cases total maximulm 27 (blgrabs mod)
		/// </summary>
		public static char GetLetter(int index, int total) {
			char letter;

			if (index == -1 || index == 0){
				letter = '0';
			} else {
				letter = Letters[index - 1];
			}
			return letter;
		}
	}
}
