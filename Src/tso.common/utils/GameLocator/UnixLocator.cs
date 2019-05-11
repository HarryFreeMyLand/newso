using System;
using System.IO;
using FSO.Compat;

namespace FSO.Common.Utils.GameLocator
{
    public class UnixLocator : IGameLocation
    {
        /// <summary>
        /// Expects The Sims Online to be located in
        /// /home/<USER_NAME>/The Sims Online/TSOClient on macOS
        /// or /game/TSOClient on Linux.
        /// </summary>
        public string FindTheSimsOnline
        {
            get
            {
                var localDir = "";

                if (PlatformDetect.IsMacOS)
                    localDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}//Documents//The Sims Online//TSOClient//";
                else if (PlatformDetect.IsLinux)
                    localDir = @"game/TSOClient/";

                if (File.Exists(Path.Combine(localDir, "tuning.dat")))
                    return localDir.Replace("\\", "/");
                else
                    throw new DirectoryNotFoundException();
            }
        }
    }
}
