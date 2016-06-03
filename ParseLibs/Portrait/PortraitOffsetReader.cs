using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.IO;
using System.Drawing;

namespace Parsers.Portrait {
	public class PortraitOffsetReader {

		private static readonly ILog logger = LogManager.GetLogger(typeof(PortraitOffsetReader).Name);

		/// <summary>
		/// GFX_byzantine_male_mouth 75x49
		/// </summary>
		public Dictionary<string, Point> Offsets = new Dictionary<string, Point>();

		public void Parse(string filename) {
			if (!File.Exists(filename)) {
				logger.Error(string.Format("File not found: {0}", filename));
				return;
			}

			string line;
			using (StreamReader reader = new StreamReader(filename, Encoding.GetEncoding(1252))) {
				while ((line = reader.ReadLine()) != null) {
					if (line.StartsWith("#"))
						continue;

					parseLine(filename, line);
				}
			}
		}

		private void parseLine(string filename, string line) {
			string[] words = line.Split(' ');
			if (words.Length == 2) {
				Point offset = new Point();
				string layerName = words[0];

				offset.X = int.Parse(words[1].Split('x')[0]);
				offset.Y = int.Parse(words[1].Split('x')[1]);
				if (Offsets.ContainsKey(layerName)) {
					if (offset != Offsets[layerName]) {
						logger.Warn(string.Format("Duplicate offsets for {0}: {1} and {2} (ignored)", layerName, Offsets[layerName], offset));
					}
				} else {
					Offsets.Add(layerName, offset);
				}
			} else {
				logger.Error(string.Format("Syntax error in file {0} on line: {1} ", filename, line));
			}
		}
	}
}
