using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsers.Portrait {

	/// <summary>
	/// Input data (dna and properties) used to display a portrait.
	/// </summary>
	public class Portrait {

		public static List<char> Letters = new List<char> { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

		private static Random rand = new Random();

		private string dna = "aaaaa0aaaa0";

		private string properties = "aa0aaaaaaaab";

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

		public char GetLetter(Characteristic characteristic) {
			char letter;
			if (characteristic.type == Characteristic.Type.DNA) {
				letter = GetDNA()[characteristic.index];
			}
			else {
				letter = GetProperties()[characteristic.index]; ;
			}
			return letter;
		}


		public override string ToString() {

			return string.Format("DNA: {0}, Properties: {1}", GetDNA(), GetProperties());
		}

		/// <summary>
		/// Converts a letter to an index
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

		public static char GetLetter(int index, int total) {
			char letter;

			if (index == 0)
				// TODO Handle the case were total = 27 (blgrabs)
				letter = Letters[(total - 1)%26];
			else if (index == -1)
				letter = '0';
			else
				letter = Letters[index - 1];

			return letter;
		}

		/// <summary>
		/// Converts a letter to an index.
		/// In case letter is 0, uses a random index
		/// </summary>
		/// <param name="letter"></param>
		/// <param name="total"></param>
		/// <returns></returns>
		public static int GetTileIndexFromLetter(char letter, int total) {
			if (letter == '0') {
				return rand.Next(total);
			}
			else {
				return GetIndex(letter, total);
			}
		}
	}
}
