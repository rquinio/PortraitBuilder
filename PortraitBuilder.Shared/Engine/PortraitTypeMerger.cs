using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;

using PortraitBuilder.Model.Portrait;

namespace PortraitBuilder.Engine {

	/// <summary>
	/// Note: should probably have used a decorator, but uneasy due to lack of interfaces in current code.
	/// </summary>
	public class PortraitTypeMerger {

		private static readonly ILog logger = LogManager.GetLogger(typeof(PortraitTypeMerger));

		/// <summary>
		/// Returns a new PortraitTytpe, with the clothing layers of basePortraitType overriden by clothingPortraitType
		/// </summary>
		public PortraitType merge(PortraitType basePortraitType, PortraitType clothingPortraitType) {
			PortraitType mergedPortraitType = new PortraitType();

			mergedPortraitType.EffectFile = basePortraitType.EffectFile;
			mergedPortraitType.HairColourIndex = basePortraitType.HairColourIndex;
			mergedPortraitType.HairColours = basePortraitType.HairColours;
			mergedPortraitType.HeadgearThatHidesHair = basePortraitType.HeadgearThatHidesHair;
			mergedPortraitType.EyeColours = basePortraitType.EyeColours;
			mergedPortraitType.EyeColourIndex = basePortraitType.EyeColourIndex;

			mergedPortraitType.Name = basePortraitType.Name + "/" + clothingPortraitType.Name;

			mergedPortraitType.Layers = new List<Layer>(basePortraitType.Layers.Count);

			foreach (Layer baseLayer in basePortraitType.Layers) {
				Layer mergeLayer = baseLayer;
				if(baseLayer.CultureIndex != -1){
					Layer overrideLayer = clothingPortraitType.GetCultureLayer(baseLayer.CultureIndex);
					if (overrideLayer != null) {
						logger.Debug(string.Format("Overriding layer {0} with {1}", baseLayer, overrideLayer));
						mergeLayer = new Layer();
						mergeLayer.Characteristic = baseLayer.Characteristic;
						mergeLayer.CultureIndex = baseLayer.CultureIndex;
						mergeLayer.Filename = overrideLayer.Filename;
						mergeLayer.IsEye = baseLayer.IsEye;
						mergeLayer.IsHair = baseLayer.IsHair;
						mergeLayer.Name = overrideLayer.Name;
						mergeLayer.Offset = baseLayer.Offset;
					}
				}
				mergedPortraitType.Layers.Add(mergeLayer);
			}

			return mergedPortraitType;
		}
	}
}
