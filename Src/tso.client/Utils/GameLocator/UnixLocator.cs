using System;
using System.IO;
using FSO.Compat;

namespace FSO.Client.Utils.GameLocator
{
    public class UnixLocator : ILocator
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
                if (PlatformDetect.IsMacOS)
                    return $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}//Documents//The Sims Online//TSOClient//";
                else if (PlatformDetect.IsLinux)
                    return @"game/TSOClient/";
                else
                    throw new DirectoryNotFoundException();
            }
        }
    }
}
