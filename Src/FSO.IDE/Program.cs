using FSO.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FSO.Client.Diagnostics;
using FSO.Files.Formats.IFF;

namespace FSO.IDE
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (!Client.Program.InitWithArguments(args))
                return;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            (new VolcanicStartProxy()).Start();
        }
    }

    class VolcanicStartProxy
    {
        public void Start()
        {
            IffFile.RETAIN_CHUNK_DATA = true;
            IDEHook.IDE = new IDETester();
            new GameStartProxy().Start(Client.Program.UseDX);
        }
    }
}
