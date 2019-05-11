using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace FSO.Common.Utils.GameLocator
{
    /// <summary>
    /// Detects The Sims Online via. the Windows' Registery
    /// </summary>
    public class WindowsLocator : IGameLocation
    {
        public string FindTheSimsOnline
        {
            get
            {
                string Software = "";

                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    //Find the path to TSO on the user's system.
                    var softwareKey = hklm.OpenSubKey("SOFTWARE");

                    if (Array.Exists(softwareKey.GetSubKeyNames(), delegate (string s)
                    { return s.Equals("Maxis", StringComparison.InvariantCultureIgnoreCase); }))
                    {
                        var maxisKey = softwareKey.OpenSubKey("Maxis");
                        if (Array.Exists(maxisKey.GetSubKeyNames(), delegate (string s)
                        { return s.Equals("The Sims Online", StringComparison.InvariantCultureIgnoreCase); }))
                        {
                            var tsoKey = maxisKey.OpenSubKey("The Sims Online");
                            string installDir = (string)tsoKey.GetValue("InstallDir");
                            installDir += @"\TSOClient\";
                            return installDir.Replace('\\', '/');
                        }
                    }
                }
                // Search relative directory similar to how macOS and Linux works; allows portability
                string localDir = @"The Sims Online\TSOClient\";
                if (File.Exists(Path.Combine(localDir, "tuning.dat")))
                    return localDir.Replace("\\", "/");

                // Never assume the game is installed
                throw new DirectoryNotFoundException();
            }
        }

        static bool _is64BitProcess = IntPtr.Size == 8;
        static bool _is64BitOperatingSystem = _is64BitProcess || InternalCheckIsWow64();

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        /// <summary>
        /// Determines if this process is run on a 64bit OS.
        /// </summary>
        /// <returns>True if it is, false otherwise.</returns>
        public static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (var p = Process.GetCurrentProcess())
                {
                    if (!IsWow64Process(p.Handle, out var retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
