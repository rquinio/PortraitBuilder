using System;
using System.ComponentModel;

namespace PortraitBuilder.Model.Portrait
{

    /// <summary>
    /// Represents one DNA or Property element
    /// </summary>
    public class Characteristic : INotifyPropertyChanged
    {

        public Characteristic(string name, int index, Type type, bool randomizable)
        {
            this.name = name;
            this.index = index;
            this.type = type;
            this.randomizable = randomizable;
        }

        public Characteristic(string name, int index, Type type, bool randomizable, bool custom)
            : this(name, index, type, randomizable)
        {
            this.custom = custom;
        }

        private string name;

        /// <summary>
        /// Index of attribute in dna/properties string
        /// </summary>
        public int index;

        public Type type;

        /// <summary>
        /// Whether the characteristic should be randomized when generating a random portrait.
        /// </summary>
        private bool randomizable;

        public bool Randomizable
        {
            get { return randomizable; }
            set
            {
                randomizable = value;
                InvokePropertyChanged(new PropertyChangedEventArgs("Randomizable"));
            }
        }

        /// <summary>
        /// Whether the characteristic is a non-vanilla one.
        /// </summary>
        public bool custom = false;

        public enum Type
        {
            DNA,
            Property
        }

        public override bool Equals(object obj)
            => !(obj is Characteristic characteristic) ? false : index.Equals(characteristic.index) && type.Equals(characteristic.type);

        public override int GetHashCode() => type.GetHashCode() + index;

        public override string ToString() => $"{name} ({(type == Type.DNA ? 'd' : 'p')}{index})";

        public static Characteristic BACKGROUND = new Characteristic("Background", 0, Type.Property, true);
        public static Characteristic HAIR = new Characteristic("Hair", 1, Type.Property, true);
        public static Characteristic HEAD = new Characteristic("Head", 2, Type.Property, true);
        public static Characteristic CLOTHES = new Characteristic("Clothes", 3, Type.Property, true);
        public static Characteristic BEARD = new Characteristic("Beard", 4, Type.Property, true);
        public static Characteristic HEADGEAR = new Characteristic("Headgear", 5, Type.Property, true);
        public static Characteristic IMPRISONED = new Characteristic("Imprisoned", 6, Type.Property, false);
        public static Characteristic SCARS = new Characteristic("Scars", 7, Type.Property, false);
        public static Characteristic RED_DOTS = new Characteristic("Reddots", 8, Type.Property, false);
        public static Characteristic BOILS = new Characteristic("Boils", 9, Type.Property, false);
        public static Characteristic BLINDED = new Characteristic("Blinded", 10, Type.Property, false);
        public static Characteristic PLAYER = new Characteristic("Player", 11, Type.Property, false);
        public static Characteristic MASK = new Characteristic("Mask", 12, Type.Property, false);
        public static Characteristic EYEPATCH = new Characteristic("Eyepatch", 13, Type.Property, false);
        public static Characteristic MAKEUP = new Characteristic("Makeup", 14, Type.Property, false);
        public static Characteristic MAKEUP_2 = new Characteristic("Makeup2", 15, Type.Property, false);
        public static Characteristic JEWELRY = new Characteristic("Jewelry", 16, Type.Property, false);
        public static Characteristic IMMORTALITY = new Characteristic("Immortality", 17, Type.Property, false);
        public static Characteristic SPECIAL_CROWN_BEHIND = new Characteristic("Crown behind", 18, Type.Property, false);
        public static Characteristic SPECIAL_CROWN = new Characteristic("Crown", 19, Type.Property, false);
        public static Characteristic FRECKLES = new Characteristic("Freckles", 20, Type.Property, false);
        public static Characteristic PHYSIQUE = new Characteristic("Physique", 21, Type.Property, false);
        public static Characteristic PALE = new Characteristic("Pale", 22, Type.Property, false);
        public static Characteristic BLACK_EYE = new Characteristic("Black eye", 23, Type.Property, false);
        public static Characteristic HAIRELIP = new Characteristic("Hairelip", 24, Type.Property, false);
        public static Characteristic SCARS_MID = new Characteristic("Scars mid", 25, Type.Property, false);
        public static Characteristic SCARS_HIGH = new Characteristic("Scars high", 26, Type.Property, false);
        public static Characteristic BLOOD = new Characteristic("Blood", 27, Type.Property, false);
        public static Characteristic TATTOO = new Characteristic("Tattoo", 28, Type.Property, false);
        public static Characteristic WARPAINT = new Characteristic("Warpaint", 29, Type.Property, false);
        public static Characteristic POSSESSED = new Characteristic("Possessed", 30, Type.Property, false);
        public static Characteristic OVERLAYER_BEHIND = new Characteristic("Overlayer behind", 31, Type.Property, false);
        public static Characteristic OVERLAYER = new Characteristic("Overlayer", 32, Type.Property, false);
        public static Characteristic UNDERMAIN = new Characteristic("Undermain", 33, Type.Property, false);
        public static Characteristic SPECIAL_HELMET = new Characteristic("Helmet", 34, Type.Property, false);
        public static Characteristic SPECIAL_MASK = new Characteristic("Mask", 35, Type.Property, false);
        public static Characteristic SPECIAL_SCEPTER = new Characteristic("Scepter", 36, Type.Property, false);


        public static Characteristic NECK = new Characteristic("Neck", 0, Type.DNA, true);
        public static Characteristic CHIN = new Characteristic("Chin", 1, Type.DNA, true);
        public static Characteristic MOUTH = new Characteristic("Mouth", 2, Type.DNA, true);
        public static Characteristic NOSE = new Characteristic("Nose", 3, Type.DNA, true);
        public static Characteristic CHEEKS = new Characteristic("Cheeks", 4, Type.DNA, true);
        public static Characteristic D5 = new Characteristic("Unused", 5, Type.DNA, true);
        public static Characteristic EYES = new Characteristic("Eyes", 6, Type.DNA, true);
        public static Characteristic EARS = new Characteristic("Ears", 7, Type.DNA, true);
        public static Characteristic HAIR_COLOR = new Characteristic("Haircolor", 8, Type.DNA, true);
        public static Characteristic EYE_COLOR = new Characteristic("Eyecolor", 9, Type.DNA, true);
        public static Characteristic D10 = new Characteristic("Unused", 10, Type.DNA, true);

        public static Characteristic[] DNA = new Characteristic[] { NECK, CHIN, MOUTH, NOSE, CHEEKS, D5, EYES, EARS, HAIR_COLOR, EYE_COLOR, D10 };
        public static Characteristic[] PROPERTIES = new Characteristic[] { BACKGROUND, HAIR, HEAD, CLOTHES, BEARD, HEADGEAR, IMPRISONED, SCARS, RED_DOTS, BOILS,
            BLINDED, PLAYER, MASK, EYEPATCH, MAKEUP, MAKEUP_2, JEWELRY, IMMORTALITY, SPECIAL_CROWN_BEHIND, SPECIAL_CROWN, FRECKLES, PHYSIQUE, PALE, BLACK_EYE,
            HAIRELIP, SCARS_MID, SCARS_HIGH, BLOOD, TATTOO, WARPAINT, POSSESSED, OVERLAYER_BEHIND, OVERLAYER, UNDERMAIN, SPECIAL_HELMET, SPECIAL_MASK, SPECIAL_SCEPTER };


        public static Characteristic getProperty(int index)
        {
            if (index < PROPERTIES.Length)
            {
                return PROPERTIES[index];
            }
            else
            {
                // As defines are not parsed, always consider custom properties as valid.
                return new Characteristic("Custom", index, Type.Property, false, true);
            }
        }

        public static Characteristic getDna(int index)
        {
            try
            {
                return DNA[index];
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException($"Characteristic d{index} does not exist.");
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokePropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

        #endregion
    }
}
