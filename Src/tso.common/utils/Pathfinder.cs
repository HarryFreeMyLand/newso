using System;
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
                    // Catch-all for Unix and Linux-based systems
                    case PlatformID.Unix:
                        gameLocation = new UnixLocator();
                        break;
                }

                return gameLocation.FindTheSimsOnline;
            }
        }
    }
}
