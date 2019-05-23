using System;
using System.IO;
using FSO.Common.Utils.GameLocator;
using FSO.Compat;

namespace FSO.Common.Utils
{
    public class Pathfinder
    {
        static string _gameLocation;
        public static string GamePath
        {
            set
            {
                _gameLocation = value;
            }
            get
            {
                IGameLocation tsoLocation;

                switch (PlatformDetect.IsPlatformID)
                {
                    default:
                    case PlatformID.Win32NT:
                        tsoLocation = new WindowsLocator();
                        break;
                    case PlatformID.MacOSX: // Deprecated in .NET Standard
                    case PlatformID.Unix:
                        tsoLocation = new UnixLocator();
                        break;
                }

                if (Directory.Exists(tsoLocation.FindTheSimsOnline))
                    _gameLocation = tsoLocation.FindTheSimsOnline;
                else
                    throw new DirectoryNotFoundException();


                return _gameLocation;
            }
        }
    }
}
