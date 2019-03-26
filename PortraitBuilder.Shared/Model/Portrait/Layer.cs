using System.Drawing;

namespace PortraitBuilder.Model.Portrait
{

    /// <summary>
    /// Portrait images are composed of a succession of layers
    /// 
    /// Note: multiple layers may contribute to render the same portrait characteristic (E.g. clothes_behind, clothes, clothes_infront).
    /// </summary>
    public class Layer
    {
        /// <summary>
        /// The name of the layer (E.g GFX_character_background)
        /// </summary>
        public string Name;

        /// <summary>
        /// The characteristic associated to this layer.
        /// 
        /// Used to determine which variation will be used within the asset sprite
        /// </summary>
        public Characteristic Characteristic;

        /// <summary>
        /// The culture index. 
        /// 
        /// Used to override clothing dynamically, based on another portraitType definition, for instance when a character changes its graphical culture via event.
        /// 
        /// 0 = clothes behind
        /// 1 = headgear behind
        /// 2 = clothes
        /// 3 = headgear mid
        /// 4 = clothes infront
        /// 5 = headgear
        /// 6 = headgear behind hairlayer
        /// 7 = headgear hairlayer
        /// </summary>
        public int CultureIndex = -1;

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

        public override string ToString() => $"{Name}:{Characteristic} from {Filename}";
    }
}