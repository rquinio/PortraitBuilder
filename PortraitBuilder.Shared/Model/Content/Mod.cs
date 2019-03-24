using System.Collections.Generic;

namespace PortraitBuilder.Model.Content
{
    public class Mod : Content
    {

        /// <summary>
        /// Name of .mod file, E.g. mymod.mod
        /// </summary>
        public string ModFile;

        /// <summary>
        /// Relative path of the mod root content folder, E.g. mod/mymod/
        /// </summary>
        public string Path;

        /// <summary>
        /// Mod user_dir optional property
        /// </summary>
        public string UserDir;

        public List<string> Dependencies = new List<string>();
        public List<string> Extends = new List<string>();
        public List<string> Replaces = new List<string>();
    }
}