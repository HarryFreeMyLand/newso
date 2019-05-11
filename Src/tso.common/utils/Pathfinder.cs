using System;
using System.IO;
using FSO.Common.Utils.GameLocator;
using FSO.Compat;

namespace FSO.Common.Utils
{
    public class Pathfinder
    {
        public static string GamePath
        {
            get
            {
                IGameLocation gameLocation;

                switch (PlatformDetect.IsPlatformID)
                {
                    default:
                    case PlatformID.Win32NT:
                        gameLocation = new WindowsLocator();
                        break;
                    case PlatformID.MacOSX: // Deprecated in .NET Standard
                    case PlatformID.Unix:
                        gameLocation = new UnixLocator();
                        break;
                }

                try
                {
                    return gameLocation.FindTheSimsOnline;
                }
                catch (DirectoryNotFoundException err)
                {
                    throw err;
                }
            }
        }
    }
}
