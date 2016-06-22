using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsers.Portrait {
	public class Portrait {

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

		public override string ToString() {
		
			return string.Format("DNA: {0}, Properties: {1}", GetDNA(), GetProperties());
		}
	}
}
