/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using Charvatia.Properties;
using FSO.SimAntics;
using FSO.SimAntics.Engine.TSOTransaction;
using FSO.SimAntics.Marshals;
using FSO.SimAntics.Model;
using FSO.SimAntics.NetPlay;
using FSO.SimAntics.NetPlay.Drivers;
using FSO.SimAntics.NetPlay.Model;
using FSO.SimAntics.NetPlay.Model.Commands;
using FSO.SimAntics.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Charvatia
{
    public class CVMInstance
    {
        public VM State;
        Stopwatch _timeKeeper;
        int _ticksSinceSave;
        static readonly int _saveTickFreq = 60 * 60; //save every minute for safety
        readonly int _port;

        public CVMInstance(int port)
        {
            VM.UseWorld = false;
            _port = port;
            ResetVM();
        }

        public void LoadState(VM vm, string path)
        {
            using (var file = new BinaryReader(File.OpenRead(path)))
            {
                var marshal = new VMMarshal();
                marshal.Deserialize(file);
                vm.Load(marshal);
                CleanLot();
                vm.Reset();
            }
        }

        public void ResetVM()
        {
            VMNetDriver driver;
            driver = new VMServerDriver(_port, NetClosed);

            var vm = new VM(new VMContext(null), driver, new VMNullHeadlineProvider());
            State = vm;
            vm.Init();
            vm.OnChatEvent += Vm_OnChatEvent;

            var path = Settings.Default.GamePath + "housedata/blueprints/" + Settings.Default.DebugLot;
            string filename = Path.GetFileName(path);
            try
            {
                //try to load from FSOV first.
                LoadState(vm, "Content/LocalHouse/" + filename.Substring(0, filename.Length - 4) + ".fsov");
            }
            catch (Exception)
            {
                try
                {
                    Console.WriteLine("Failed FSOV load... Trying Backup");
                    LoadState(vm, $"Content/LocalHouse/{filename.Substring(0, filename.Length - 4)}_backup.fsov");
                }
                catch (Exception)
                {
                    Console.WriteLine("CRITICAL::: Failed FSOV load... Trying Blueprint (first run or something went wrong)");
                    short jobLevel = -1;

                    //quick hack to find the job level from the chosen blueprint
                    //the final server will know this from the fact that it wants to create a job lot in the first place...

                    try
                    {
                        if (filename.StartsWith("nightclub") || filename.StartsWith("restaurant") || filename.StartsWith("robotfactory"))
                            jobLevel = Convert.ToInt16(filename.Substring(filename.Length - 9, 2));
                    }
                    catch (Exception) { }

                    vm.SendCommand(new VMBlueprintRestoreCmd
                    {
                        JobLevel = jobLevel,
                        XMLData = File.ReadAllBytes(path)
                    });

                    vm.Context.Clock.Hours = 10;
                }
            }
            vm.MyUID = uint.MaxValue - 1;
            vm.SendCommand(new VMNetSimJoinCmd
            {
                ActorUID = uint.MaxValue - 1,
                Name = "server"
            });
        }

        void CleanLot()
        {
            Console.WriteLine("Cleaning up the lot...");
            var avatars = new List<VMEntity>(State.Entities.Where(x => x is VMAvatar && x.PersistID > 65535));
            //TODO: all avatars with persist ID are not npcs in TSO. right now though everything has a persist ID...
            //step 1, force everyone to leave.
            foreach (var avatar in avatars)
                State.ForwardCommand(new VMNetSimLeaveCmd()
                {
                    ActorUID = avatar.PersistID,
                    FromNet = false
                });

            //simulate for a bit to try get rid of the avatars on the lot
            try
            {
                for (int i = 0; i < 30 * 60 && State.Entities.FirstOrDefault(x => x is VMAvatar && x.PersistID > 65535) != null; i++)
                {
                    if (i == 30 * 60 - 1)
                        Console.WriteLine("Failed to clean lot...");
                    State.Update();
                }
            }
            catch (Exception) { } //if something bad happens just immediately try to delete everyone

            avatars = new List<VMEntity>(State.Entities.Where(x => x is VMAvatar && (x.PersistID > 65535 || (!(x as VMAvatar).IsPet))));
            foreach (var avatar in avatars)
                avatar.Delete(true, State.Context);
        }

        void NetClosed(VMCloseNetReason reason)
        {
            if (reason != VMCloseNetReason.ServerShutdown)
                return; //only handle clean closes
            var server = State.GetObjectByPersist(uint.MaxValue - 1);
            if (server != null)
                server.Delete(true, State.Context);
            CleanLot();
            SaveLot();
            Environment.Exit(0);
        }

        void Vm_OnChatEvent(VMChatEvent evt)
        {
            var print = "";
            switch (evt.Type)
            {
                case VMChatEventType.Message:
                    print = "<%> says: ".Replace("%", evt.Text[0]) + evt.Text[1];
                    break;
                case VMChatEventType.MessageMe:
                    print = "You say: " + evt.Text[1];
                    break;
                case VMChatEventType.Join:
                    print = "<%> has entered the property.".Replace("%", evt.Text[0]);
                    break;
                case VMChatEventType.Leave:
                    print = "<%> has left the property.".Replace("%", evt.Text[0]);
                    break;
                case VMChatEventType.Arch:
                    print = "<" + evt.Text[0] + " (" + evt.Text[1] + ")" + "> " + evt.Text[2];
                    break;
                case VMChatEventType.Generic:
                    print = evt.Text[0];
                    break;
            }

            Console.WriteLine(print);
        }

        public void SendMessage(string msg)
        {
            State.SendCommand(new VMNetChatCmd
            {
                ActorUID = uint.MaxValue - 1,
                Message = msg
            });
        }

        public void Start()
        {
            var oThread = new Thread(new ThreadStart(TickVM));
            oThread.Start();
        }

        public void SaveLot()
        {
            string filename = Path.GetFileName(Settings.Default.DebugLot);
            var exporter = new VMWorldExporter();
            exporter.SaveHouse(State, Path.Combine(Settings.Default.GamePath + "housedata/blueprints/" + filename));

            var marshal = State.Save();
            Directory.CreateDirectory("Content/LocalHouse/");
            var extensionless = filename.Substring(0, filename.Length - 4);

            //backup old state
            try
            { File.Copy("Content/LocalHouse/" + extensionless + ".fsov", "Content/LocalHouse/" + extensionless + "_backup.fsov", true); }
            catch (Exception) { }

            using (var output = new FileStream("Content/LocalHouse/" + extensionless + ".fsov", FileMode.Create))
            {
                marshal.SerializeInto(new BinaryWriter(output));
            }

            ((VMTSOGlobalLinkStub)State.GlobalLink).Database.Save();
        }

        void TickVM()
        {
            _timeKeeper = new Stopwatch();
            _timeKeeper.Start();
            long lastMs = 0;
            while (true)
            {
                lastMs += 16;
                _ticksSinceSave++;
                try
                {
                    State.Update();
                }
                catch (Exception e)
                {
                    State.CloseNet(VMCloseNetReason.Unspecified);
                    Console.WriteLine(e.ToString());
                    SaveLot();
                    Thread.Sleep(500);

                    ResetVM();
                    //restart on exceptions... but print them to console
                    //just for people who like 24/7 servers.
                }

                if (_ticksSinceSave > _saveTickFreq)
                {
                    //quick and dirty periodic save
                    SaveLot();
                    _ticksSinceSave = 0;
                }

                Thread.Sleep((int)Math.Max(0, (lastMs + 16) - _timeKeeper.ElapsedMilliseconds));
            }
        }
    }
}
