using System.IO;
using System.Linq;
using PortraitBuilder.Model.Portrait;

namespace PortraitBuilder.Model.Content
{

    /// <summary>
    /// Represents a loadable unit of content (vanilla, DLC, mod, ...).
    /// </summary>
    public class Content
    {

        /// <summary>
        /// Name to be displayed, E.g. "My Mod"
        /// </summary>
        public string Name;

        /// <summary>
        /// Absolute path to the content root folder
        /// </summary>
        public string AbsolutePath;

        public string PortraitPath => Path.Combine(AbsolutePath, @"gfx\characters\");

        public PortraitData PortraitData;

        /// <summary>
        /// Watcher on content data changes
        /// </summary>
        public FileSystemWatcher Watcher;

        /// <summary>
        /// Whether the content is supported by this tool and can be enabled
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// Reason why the content is not enabled.
        /// </summary>
        public string DisabledReason;

        public override string ToString() => Name;

        /// <summary>
        /// True if either:
        /// - content has portraitTypes .gfx definitions
        /// - or some sprites are overriden (ex: Mediteranean Portraits DLC)
        /// 
        /// Checking any kind of sprites would cause too many false positives (unit DLCs, ...).
        /// </summary>
        /// <returns></returns>
        public bool HasPortraitData => PortraitData?.PortraitTypes.Any() == true || Directory.Exists(PortraitPath);

        public void Unload()
        {
            PortraitData?.Unload();
        }

        public void Dispose()
        {
            PortraitData?.Dispose();
        }
    }
}
