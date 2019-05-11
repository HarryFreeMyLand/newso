using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using FSO.Client.Utils;
using FSO.Client.Utils.GameLocator;
using FSO.Common;
using FSO.UI;
using FSO.Compat;
//using System.Windows.Forms;

namespace FSO.Client
{
    public class FSOProgram : IFSOProgram
    {
        public bool UseDX { get; set; }

        public static Action<string> ShowDialog = DefaultShowDialog;

        public static void DefaultShowDialog(string text)
        {
            Console.WriteLine(text);
        }

        public bool InitWithArguments(string[] args)
        {
            // Added by Harrison, needs to run pretty early to avoid exception.
            NinjectCheck();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(baseDir);
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            var os = Environment.OSVersion;
            var pid = os.Platform;

            ILocator gameLocator;
            bool linux = pid == PlatformID.MacOSX || pid == PlatformID.Unix;

            switch (PlatformDetect.IsPlatformID)
            {
                case PlatformID.Unix:
                    gameLocator = new UnixLocator();
                    break;
                default:
                case PlatformID.Win32NT:
                    gameLocator = new WindowsLocator();
                    break;
            }

            bool useDX = false;

            #region User resolution parmeters

            foreach (var arg in args)
            {
                if (char.IsDigit(arg[0]))
                {
                    //attempt parsing resoulution
                    try
                    {
                        var split = arg.Split("x".ToCharArray());
                        int ScreenWidth = int.Parse(split[0]);
                        int ScreenHeight = int.Parse(split[1]);

                        GlobalSettings.Default.GraphicsWidth = ScreenWidth;
                        GlobalSettings.Default.GraphicsHeight = ScreenHeight;
                    }
                    catch (Exception) { }
                }
                else if (arg[0] == '-')
                {
                    var cmd = arg.Substring(1);
                    if (cmd.StartsWith("lang"))
                    {
                        GlobalSettings.Default.LanguageCode = byte.Parse(cmd.Substring(4));
                    }
                    else if (cmd.StartsWith("hz"))
                        GlobalSettings.Default.TargetRefreshRate = int.Parse(cmd.Substring(2));
                    else
                    {
                        //normal style param
                        switch (cmd)
                        {
                            case "dx11":
                            case "dx":
                                useDX = true;
                                break;
                            case "gl":
                            case "ogl":
                                useDX = false;
                                break;
                            case "ts1":
                                GlobalSettings.Default.TS1HybridEnable = true;
                                break;
                            case "tso":
                                GlobalSettings.Default.TS1HybridEnable = false;
                                break;
                            case "3d":
                                FSOEnvironment.Enable3D = true;
                                break;
                            case "touch":
                                FSOEnvironment.SoftwareKeyboard = true;
                                break;
                            case "nosound":
                                FSOEnvironment.NoSound = true;
                                break;
                        }
                    }
                }
                else
                {
                    if (arg.Equals("w", StringComparison.InvariantCultureIgnoreCase))
                        GlobalSettings.Default.Windowed = true;
                    else if (arg.Equals("f", StringComparison.InvariantCultureIgnoreCase))
                        GlobalSettings.Default.Windowed = false;
                }
            }

            #endregion

            UseDX = MonogameLinker.Link(useDX);

            var path = gameLocator.FindTheSimsOnline;

            if (path != null)
            {
                //check if this path has tso in it. tuning.dat should be a good indication.
                if (!File.Exists(Path.Combine(path, "tuning.dat")))
                {
                    ShowDialog($"The Sims Online appears to be missing. The game expects TSO at directory '{path}', but some core files are missing from that folder. If you know you installed TSO into a different directory, please move it into the directory specified.");
                    return false;
                }

                FSOEnvironment.Args = string.Join(" ", args);
                FSOEnvironment.ContentDir = "Content/";
                FSOEnvironment.GFXContentDir = $"Content/{(UseDX ? "DX/" : "OGL/")}";
                FSOEnvironment.Linux = linux;
                FSOEnvironment.DirectX = UseDX;
                FSOEnvironment.GameThread = Thread.CurrentThread;
                if (GlobalSettings.Default.LanguageCode == 0)
                    GlobalSettings.Default.LanguageCode = 1;
                Files.Formats.IFF.Chunks.STR.DefaultLangCode = (Files.Formats.IFF.Chunks.STRLangCode)GlobalSettings.Default.LanguageCode;

                GlobalSettings.Default.StartupPath = path;
                GlobalSettings.Default.ClientVersion = ClientVersion;
                return true;
            }
            else
            {
                ShowDialog($"The Sims Online was not found on your system. {GameConsts.GameName} will not be able to run without access to the original game files.");
                return false;
            }
        }

        static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var assemblyPath = Path.Combine(MonogameLinker.AssemblyDir, args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll");
                var assembly = Assembly.LoadFrom(assemblyPath);
                return assembly;
            }
            catch (Exception)
            {
                return null;
            }

        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
        }

        string ClientVersion
        {
            get
            {
                /*
                string ExeDir = GlobalSettings.Default.StartupPath;

                if (File.Exists("version.txt"))
                {
                    using (StreamReader Reader = new StreamReader(File.Open("version.txt", FileMode.Open, FileAccess.Read, FileShare.Read)))
                    {
                        return Reader.ReadLine();
                    }
                }
                else
                {
                    return "(?)";
                }
                */
                return GameConsts.TCVersion;
            }
        }

        // Added by Harrison
        /// <summary>
        /// Checks the version and if it's not the right version, replace it.
        /// </summary>
        void NinjectCheck()
        {
            var NinjectVersion = FileVersionInfo.GetVersionInfo(@"ninject.dll");

            if (!NinjectVersion.FileVersion.Contains("3.3"))
            {
                File.Copy(@".\\x86\\ninject.dll", @".\\ninject.dll", true);
            }
        }
    }
}
