using System.Collections.Generic;
using System.Drawing;

namespace Parsers.Portrait {

	/// <summary>
	/// Portrait images are composed of a succession of layers
	/// 
	/// Note: multiple layers may contribute to render the same portrait characteristic (E.g. clothes_behind, clothes, clothes_infront).
	/// </summary>
	public class Layer {
		/// <summary>
		/// The name of the layer (E.g GFX_character_background)
		/// </summary>
		public string Name;

		/// <summary>
		/// The characteristic associated to this layer.
		/// 
		/// It used to determine which variation will be used within the asset sprite
		/// </summary>
		public Characteristic Characteristic;

		/// <summary>
		/// Whether this layer should apply hair coloration 
		/// 
		/// Note: used for both hair and beard layers
		/// </summary>
		public bool IsHair;

		/// <summary>
		/// Whether this layer should apply eye coloration 
		/// </summary>
		public bool IsEye;

		public bool DontRefreshIfValid = false;

		/// <summary>
		/// The offset coordinates for this layer, from the bottom-left of the image
		/// </summary>
		public Point Offset = new Point(0, 0);

		/// <summary>
		/// The file that the data was loaded from.
		/// </summary>
		public string Filename;

		public override string ToString() {
			return string.Format("{0} from {1}", Name + ":" + Characteristic, Filename);
		}
	}
}