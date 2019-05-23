using System;
using System.Runtime.InteropServices;

namespace FSO.Compat
{
    // Based on https://mariusschulz.com/blog/detecting-the-operating-system-in-net-core
    public class PlatformDetect
    {
        public static PlatformID IsPlatformID
        {
            get
            {
                if (IsWindows)
                    return PlatformID.Win32NT;
                else
                    return PlatformID.Unix;
            }
        }

        public static bool IsWindows
        {
            get
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            }
        }

        public static bool IsMacOS
        {
            get
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            }
        }

        public static bool IsLinux
        {
            get
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            }
        }
    }
}
