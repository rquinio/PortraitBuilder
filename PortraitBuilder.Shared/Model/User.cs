namespace PortraitBuilder.Model
{

    /// <summary>
    /// User configuration
    /// </summary>
    public class User
    {
        /// <summary>
        /// Path of the Crusader Kings II game directory in Steam. E.g. C:\Paradox\Crusader Kings II
        /// </summary>
        public string GameDir;

        /// <summary>
        /// Path to the mod directory.
        /// </summary>
        public string ModDir;

        /// <summary>
        /// Path to the temporary DLC directory, containing unzipped assets
        /// </summary>
        public string DlcDir;

        public override string ToString()
        {
            return string.Format("CK2 directory: {0}, Mod directory: {1}, DLC directory: {2}", GameDir, ModDir, DlcDir);
        }
    }
}
