using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace FSO.Server.Patcher
{
    class Program
    {
        const int RENAME_MAX_ATTEMPTS = 8;
        const string APP_NAME = "Sim Tactics Server";

        int _renameRetry = 0;
        static readonly HashSet<string> _ignoreFiles = new HashSet<string>()
        {
            "config.json"
        };

        public async Task<bool> ExtractEntry(ZipArchiveEntry entry, int tryNum)
        {
            var name = (entry.FullName == "update.exe") ? "update2.exe" : entry.FullName;
            var targPath = Path.Combine("./", name);
            Directory.CreateDirectory(Path.GetDirectoryName(targPath));

            try
            {
                entry.ExtractToFile(targPath, true);
                Console.WriteLine($"{name} extracted...");
                return true;
            }
            catch (Exception ex)
            {
                if (ex is DirectoryNotFoundException)
                    return true;

                if (tryNum++ > 3)
                {
                    Console.WriteLine("");
                    return false;
                }
                else
                {
                    Console.WriteLine($"Waiting for {name}... {ex}");
                    await Task.Delay(3000);
                    return await ExtractEntry(entry, tryNum);
                }
            }
        }

        public async void Extract()
        {
            Console.WriteLine($"Extracting {APP_NAME} files.");
        }

        public void Cleanup()
        {
            try
            {
                var fileName = $"SimTactics.exe";
                if (File.Exists($"{fileName}.old"))
                    File.Move($"{fileName}.old", fileName);
            }
            catch
            {
                // Do nothing
            }
        }

        public void AttemptRename()
        {
            try
            {
                var fileName = $"server.exe";
                if (File.Exists(fileName))
                    File.Move(fileName, $"{fileName}.old");
            }
            catch
            {
                if (_renameRetry++ < RENAME_MAX_ATTEMPTS)
                {
                    Console.WriteLine($"Waiting for the {APP_NAME} server to close.");
                    Task.Run(async () =>
                    {
                        await Task.Delay(2000);
                        AttemptRename();
                    });
                    return;
                }
                else
                {
                    Console.WriteLine($"Could not update {APP_NAME} server as write access could not be gained to the game files. Try running update.exe as an administrator.");
                    Console.WriteLine("Continue? Y/N");
                    var input = Console.ReadLine();
                    switch (input.ToLowerInvariant())
                    {
                        case "n":
                            Cleanup();
                            Environment.Exit(Environment.ExitCode);
                            break;
                        case "y":
                        default:
                            _renameRetry = 0;
                            break;
                    }
                    return;
                }
            }
        }

        public void StartGame()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                Process.Start("mono", "server.exe run");
            }
            else
            {
                Process.Start("server.exe");
            }
            Environment.Exit(Environment.ExitCode);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
