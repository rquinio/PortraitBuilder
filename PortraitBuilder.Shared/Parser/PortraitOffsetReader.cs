using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.IO;
using System.Drawing;

namespace PortraitBuilder.Parser {

    using static EncodingHelper;
    /// <summary>
    /// Handles the parsing of portrait_offsets.txt files.
    /// </summary>
    public class PortraitOffsetReader {

		private static readonly ILog logger = LogManager.GetLogger(typeof(PortraitOffsetReader));

		/// <summary>
		/// Parse offset files
		/// </summary>
		/// <param name="filename"></param>
		/// <returns>Map sprite name / offset, E.g GFX_byzantine_male_mouth 75x49</returns>
		public Dictionary<string, Point> Parse(string filename) {
			if (!File.Exists(filename)) {
				logger.Error(string.Format("File not found: {0}", filename));
				return null;
			}

			Dictionary<string, Point> offsets = new Dictionary<string, Point>();

			string line;
			using (StreamReader reader = new StreamReader(filename, WesternEncoding)) {
				while ((line = reader.ReadLine()) != null) {
					if (line.StartsWith("#"))
						continue;

					parseLine(filename, line, offsets);
				}
			}
			return offsets;
		}

		private void parseLine(string filename, string line, Dictionary<string, Point>  offsets) {
			string[] words = line.Split(' ');
			if (words.Length == 2) {
				Point offset = new Point();
				string layerName = words[0];

				offset.X = int.Parse(words[1].Split('x')[0]);
				offset.Y = int.Parse(words[1].Split('x')[1]);
				if (offsets.ContainsKey(layerName)) {
					if (offset != offsets[layerName]) {
						logger.Warn(string.Format("Duplicate offsets for {0}: {1} and {2} (ignored)", layerName, offsets[layerName], offset));
					}
				} else {
					offsets.Add(layerName, offset);
				}
			} else {
				logger.Error(string.Format("Syntax error in file {0} on line: {1} ", filename, line));
			}
		}
	}
}
