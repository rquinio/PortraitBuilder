using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using PortraitBuilder.Model.Content;

namespace PortraitBuilder.Parser
{
    using static EncodingHelper;
    public class DLCReader
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(DLC));

        public List<DLC> ParseFolder(string folder)
        {
            List<DLC> dlcs = new List<DLC>();

            DirectoryInfo dir = new DirectoryInfo(folder);
            if (dir.Exists)
            {
                FileInfo[] dlcFiles = dir.GetFiles("*.dlc");
                if (dlcFiles.Length == 0)
                {
                    logger.Error(string.Format("No DLC files found in folder: {0}", dir.FullName));
                }

                foreach (FileInfo dlcFile in dlcFiles)
                {
                    DLC dlc = Parse(dlcFile.FullName);
                    if (dlc != null && dlc.Archive != null)
                    {
                        // Note: path will be overriden when extracting the archive
                        dlc.AbsolutePath = Path.Combine(folder, dlc.Archive.Substring("dlc".Length + 1)); // Remove "dlc/" from path
                        dlcs.Add(dlc);
                    }
                }
            }
            else
            {
                logger.Error(string.Format("Folder not found: {0}", dir.FullName));
            }
            return dlcs;
        }

        private DLC Parse(string filename)
        {
            if (!File.Exists(filename))
            {
                logger.Error(string.Format("File not found: {0}", filename));
                return null;
            }

            string line;
            int intOut;
            FileInfo dlcFile = new FileInfo(filename);

            DLC dlc = new DLC();
            dlc.DLCFile = dlcFile.Name;

            StreamReader reader = new StreamReader(filename, WesternEncoding);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#"))
                    continue;

                if (line.StartsWith("name"))
                    dlc.Name = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim();
                if (line.StartsWith("archive"))
                    dlc.Archive = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim();
                if (line.StartsWith("checksum"))
                    dlc.Checksum = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim();

                if (line.StartsWith("steam_id"))
                {
                    if (Int32.TryParse(line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim(), out intOut))
                        dlc.SteamID = intOut;
                    else
                        logger.Error(string.Format("Error parsing Steam ID in file: {0}", dlcFile.Name));
                }

                if (line.StartsWith("gamersgate_id"))
                {
                    if (Int32.TryParse(line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim(), out intOut))
                        dlc.GamersGateID = intOut;
                    else
                        logger.Error(string.Format("Error parsing GamersGate ID in file: {0}", dlcFile.Name));
                }

                if (line.StartsWith("affects_checksum"))
                    dlc.AffectsChecksum = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim() == "yes";
            }
            return dlc;
        }


    }
}
