using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using log4net;

namespace PortraitBuilder.Model.Portrait
{

    /// <summary>
    /// 
    /// </summary>
    public class PortraitData
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(PortraitData));

        /// <summary>
        /// Dictionary of included sprites
        /// Key is the name of the sprite. E.g. GFX_character_background
        /// </summary>
        public Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        /// <summary>
        /// Dictionary of included Portrait Types.
        /// Key is the name of the Portrait Type. E.g. PORTRAIT_westerngfx_male
        /// </summary>
        public Dictionary<string, PortraitType> PortraitTypes = new Dictionary<string, PortraitType>();

        /// <summary>
        /// Dictionary of optional external offsets 
        /// Key is the name of the sprite. E.g. GFX_byzantine_male_mouth
        /// 
        /// Note: external offsets (if any) are applied globally during the merging, and not per content.
        /// </summary>
        public Dictionary<string, Point> Offsets = new Dictionary<string, Point>();

        /// <summary>
        /// Removes all data from memory
        /// </summary>
        public void Dispose()
        {
            logger.Info("Disposing of previous portrait data.");

            Unload();

            Sprites.Clear();
            PortraitTypes.Clear();
            Offsets.Clear();
        }

        /// <summary>
        /// Unloads all loaded portrait data.
        /// </summary>
        public void Unload()
        {
            foreach (KeyValuePair<string, Sprite> pair in Sprites)
            {
                if (pair.Value.IsLoaded)
                {
                    pair.Value.Unload();
                }
            }
        }

        // Last wins implementation
        public void MergeWith(PortraitData other)
        {
            Sprites = Sprites.Concat(other.Sprites).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.Last().Value);
            PortraitTypes = PortraitTypes.Concat(other.PortraitTypes).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.Last().Value);
            Offsets = Offsets.Concat(other.Offsets).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.Last().Value);
        }

        public int GetFrameCount(PortraitType portraitType, Characteristic characteristic)
        {
            int nbTiles = 0;
            if (characteristic == Characteristic.EYE_COLOR)
            {
                nbTiles = portraitType.EyeColours.Count;
            }
            else if (characteristic == Characteristic.HAIR_COLOR)
            {
                nbTiles = portraitType.HairColours.Count;
            }
            else
            {
                foreach (Layer layer in portraitType.Layers)
                {
                    if (layer.Characteristic == characteristic)
                    {
                        if (Sprites.ContainsKey(layer.Name))
                        {
                            Sprite sprite = Sprites[layer.Name];
                            if (sprite != null)
                            {
                                nbTiles = sprite.FrameCount;
                                break;
                            }
                        }
                        else
                        {
                            logger.Error("Sprite not found for layer " + layer);
                        }
                    }
                }
            }
            return nbTiles;
        }
    }
}
