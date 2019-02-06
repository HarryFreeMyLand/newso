/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using Charvatia.Properties;
using FSO.Common;
using GonzoNet;
using ProtocolAbstractionLibraryD;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Charvatia
{
    class Program
    {
        static CVMInstance _inst;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Console.Title = $"Charvatia {GameConsts.FullVersion}";
            Init();
        }

        static void Init()
        {
            Console.WriteLine("Loading Content...");
            FSO.Content.GameContent.Init(Settings.Default.GamePath, null);
            Console.WriteLine("Success!");

            Console.WriteLine("Starting VM server...");

            PacketHandlers.Register((byte)PacketType.VM_PACKET, false, 0, new OnPacketReceive(VMPacket));

            StartVM();
            var inputStream = Console.OpenStandardInput();

            while (true)
                _inst.SendMessage(Console.ReadLine());
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var assemblyPath = Path.Combine("Monogame/Windows/", args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll");
                var assembly = Assembly.LoadFrom(assemblyPath);
                return assembly;
            }
            catch (Exception)
            {
                return null;
            }
        }

        static private void VMPacket(NetworkClient client, ProcessedPacket packet)
        {

        }

        static void StartVM()
        {
            _inst = new CVMInstance(37564);
            Console.WriteLine("Stunning success.");
            _inst.Start();
        }
    }
}
