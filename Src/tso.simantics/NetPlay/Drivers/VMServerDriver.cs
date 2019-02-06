﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.SimAntics.NetPlay.Model;
using System.IO;
using GonzoNet;
using GonzoNet.Encryption;
using System.Net;
using ProtocolAbstractionLibraryD;
using FSO.SimAntics.NetPlay.Model.Commands;
using FSO.SimAntics.NetPlay.SandboxMode;
using System.Threading;
using FSO.SimAntics.Engine.TSOTransaction;
using System.Diagnostics;

namespace FSO.SimAntics.NetPlay.Drivers
{
    public class VMServerDriver : VMNetDriver
    {
        private List<VMNetCommand> QueuedCmds;

        private const int TICKS_PER_PACKET = 2;
        private List<VMNetTick> TickBuffer;

        private Dictionary<NetworkClient, uint> ClientToUID;
        private Dictionary<uint, NetworkClient> UIDtoClient;

        private Listener listener;
        private HashSet<NetworkClient> ClientsToDC;
        private HashSet<NetworkClient> ClientsToSync;
        private event VMServerClosedHandler OnShutdown;
        public delegate void VMServerClosedHandler(VMCloseNetReason reason);

        public BanList SandboxBans;

        private uint TickID = 0;

        public VMServerDriver(int port, VMServerClosedHandler onShutdown)
        {
            listener = new Listener(EncryptionMode.NoEncryption);
            listener.Initialize(new IPEndPoint(IPAddress.Any, port));
            listener.OnConnected += SendLotState;
            listener.OnDisconnected += LotDC;

            GlobalLink = new VMTSOGlobalLinkStub(); //final server will use DB hooked class

            ClientsToDC = new HashSet<NetworkClient>();
            ClientsToSync = new HashSet<NetworkClient>();
            QueuedCmds = new List<VMNetCommand>();
            TickBuffer = new List<VMNetTick>();
            ClientToUID = new Dictionary<NetworkClient, uint>();
            UIDtoClient = new Dictionary<uint, NetworkClient>();
            if (onShutdown != null) OnShutdown += onShutdown;

            SandboxBans = new BanList();
        }

        private void LotDC(NetworkClient Client)
        {
            lock (ClientToUID) {
                if (ClientToUID.ContainsKey(Client)) {
                    uint UID = ClientToUID[Client];
                    ClientToUID.Remove(Client);
                    UIDtoClient.Remove(UID);

                    SendCommand(new VMNetSimLeaveCmd
                    {
                        ActorUID = UID
                    });
                }
            }
        }

        private void SendLotState(NetworkClient client)
        {
            lock (ClientsToSync)
            {
                ClientsToSync.Add(client);
            }
        }

        private void SendState(VM vm)
        {
            if (ClientsToSync.Count == 0) return;
            //Console.WriteLine("== SERIAL: Sending State to Client... ==");

            var watch = new Stopwatch();
            watch.Start();

            var state = vm.Save();
            //Console.WriteLine("== STATE: Intermediate - after save... " + watch.ElapsedMilliseconds + " ms. ==");
            var cmd = new VMNetCommand(new VMStateSyncCmd { State = state });

            //currently just hack this on the tick system. will change when we switch to not gonzonet
            var ticks = new VMNetTickList { Ticks = new List<VMNetTick> {
                new VMNetTick {
                    Commands = new List<VMNetCommand> { cmd },
                    RandomSeed = 0, //will be restored by client from cmd
                    TickID = TickID
                }
            } };

            byte[] data;
            using (var stream = new MemoryStream())
            {
                //Console.WriteLine("== STATE: Intermediate - before serialize... " + watch.ElapsedMilliseconds + " ms. ==");
                using (var writer = new BinaryWriter(stream))
                {
                    ticks.SerializeInto(writer);
                }

                //Console.WriteLine("== STATE: Intermediate - before toArray... " + watch.ElapsedMilliseconds + " ms. ==");
                data = stream.ToArray();
            }
            //Console.WriteLine("== STATE: Intermediate - before send... " + watch.ElapsedMilliseconds + " ms. ==");

            byte[] packet;

            using (var stream = new PacketStream((byte)PacketType.VM_PACKET, 0))
            {
                stream.WriteHeader();
                stream.WriteInt32(data.Length + (int)PacketHeaders.UNENCRYPTED);
                stream.WriteBytes(data);

                packet = stream.ToArray();
            }
            foreach (var client in ClientsToSync) client.Send(packet);
            ClientsToSync.Clear();

            watch.Stop();
            //Console.WriteLine("== SERIAL: DONE! State send took "+watch.ElapsedMilliseconds+" ms. ==");
        }

        public override void SendCommand(VMNetCommandBodyAbstract cmd)
        {
            lock (QueuedCmds)
            {
                QueuedCmds.Add(new VMNetCommand(cmd));
            }
        }

        public override bool Tick(VM vm)
        {
            HandleClients();

            lock (QueuedCmds) {
                //verify the queued commands. Remove ones which fail (or defer til later)
                for (int i=0; i<QueuedCmds.Count; i++)
                {
                    var caller = vm.GetObjectByPersist(QueuedCmds[i].Command.ActorUID);
                    if (!(caller is VMAvatar)) caller = null;
                    if (!QueuedCmds[i].Command.Verify(vm, (VMAvatar)caller)) QueuedCmds.RemoveAt(i--);
                    else if (QueuedCmds[i].Type == VMCommandType.SimJoin)
                    {
                        var cmd = (VMNetSimJoinCmd)QueuedCmds[i].Command;
                        if (cmd.Client != null)
                        {
                            lock (ClientToUID)
                            {
                                ClientToUID.Add(cmd.Client, cmd.ActorUID);
                                UIDtoClient.Add(cmd.ActorUID, cmd.Client);
                            }
                        }
                    }
                }

                var tick = new VMNetTick();
                tick.Commands = new List<VMNetCommand>(QueuedCmds);
                tick.TickID = TickID++;
                tick.RandomSeed = vm.Context.RandomSeed;
                QueuedCmds.Clear();
                InternalTick(vm, tick);
                
                TickBuffer.Add(tick);
                if (TickBuffer.Count >= TICKS_PER_PACKET)
                {
                    lock (ClientsToSync)
                    {
                        SendTickBuffer();
                        SendState(vm);
                    }
                }
            }

            return true;
        }

        private void SendTickBuffer()
        {
            var ticks = new VMNetTickList { Ticks = TickBuffer };
            byte[] data;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    ticks.SerializeInto(writer);
                }

                data = stream.ToArray();
            }

            using (var stream = new PacketStream((byte)PacketType.VM_PACKET, 0))
            {
                stream.WriteHeader();
                stream.WriteInt32(data.Length + (int)PacketHeaders.UNENCRYPTED);
                stream.WriteBytes(data);
                Broadcast(stream.ToArray(), ClientsToSync);
            }

            TickBuffer.Clear();
        }

        private void SendOneOff(NetworkClient client, VMNetTick tick) //uh, this is a little silly.
        {
            var ticks = new VMNetTickList { Ticks = new List<VMNetTick>() { tick }, ImmediateMode = true };
            byte[] data;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    ticks.SerializeInto(writer);
                }

                data = stream.ToArray();
            }

            using (var stream = new PacketStream((byte)PacketType.VM_PACKET, 0))
            {
                stream.WriteHeader();
                stream.WriteInt32(data.Length + (int)PacketHeaders.UNENCRYPTED);
                stream.WriteBytes(data);
                client.Send(stream.ToArray());
            }
        }

        private void Broadcast(byte[] packet, HashSet<NetworkClient> ignore)
        {
            lock (listener.Clients)
            {
                var clients = new List<NetworkClient>(listener.Clients);
                foreach (var client in clients)
                {
                    if (ignore.Contains(client)) continue;
                    client.Send(packet);
                }
            }
        }

        private void HandleClients()
        {
            lock (listener.Clients)
            {
                ClientsToDC.Clear();
                foreach (var client in listener.Clients)
                {
                    var packets = client.GetPackets();
                    while (packets.Count > 0)
                    {
                        OnPacket(client, packets.Dequeue());
                    }
                }
                foreach (var client in ClientsToDC)
                {
                    client.Disconnect();
                }
            }
        }

        private void SendGenericMessage(NetworkClient client, string title, string msg)
        {
            SendOneOff(client, new VMNetTick()
            {
                Commands = new List<VMNetCommand>()
                    {
                        new VMNetCommand(new VMGenericDialogCommand
                        {
                            Title = title,
                            Message = msg
                        })
                    }
            });
        }

        public override void OnPacket(NetworkClient client, ProcessedPacket packet)
        {
            var cmd = new VMNetCommand();
            try {
                using (var reader = new BinaryReader(packet)) {
                    cmd.Deserialize(reader);
                }
            } catch (Exception)
            {
                ClientsToDC.Add(client);
                return;
            }

            if (cmd.Type == VMCommandType.SimJoin)
            {
                if (SandboxBans.Contains(client.RemoteIP))
                {
                    SendGenericMessage(client, "Banned", "You have been banned from this sandbox server!");
                    ClientsToDC.Add(client);
                    return;
                }
                else if (((VMNetSimJoinCmd)cmd.Command).Version != VMNetSimJoinCmd.CurVer)
                {
                    SendGenericMessage(client, "Version Mismatch", "Your game version does not match the server's. Please update your game.");
                    ClientsToDC.Add(client);
                    return;
                }

                ((VMNetSimJoinCmd)cmd.Command).Ticket = client.RemoteIP+":"+((VMNetSimJoinCmd)cmd.Command).Name;
                ((VMNetSimJoinCmd)cmd.Command).Client = client; //remember client so we can bind it to persist id later
                SendCommand(cmd.Command); //just go for it. will start the avatar retrieval process.
                return;
            }
            else if (cmd.Type == VMCommandType.RequestResync)
            {
                lock (ClientsToSync)
                {
                    //todo: throttle
                    ClientsToSync.Add(client);
                }
            }

            lock (ClientToUID)
            {
                if (!ClientToUID.ContainsKey(client)) return; //client hasn't registered yet
                cmd.Command.ActorUID = ClientToUID[client];
            }
            SendCommand(cmd.Command);
        }

        public override void CloseNet()
        {
            listener.Close();
            if (OnShutdown != null) OnShutdown(CloseReason);
        }

        public void BanUser(VM vm, string name)
        {
            var sims = vm.Entities.Where(x => x is VMAvatar && x.ToString().ToLowerInvariant().Trim(' ') == name.ToLowerInvariant().Trim(' '));
            lock (ClientToUID) {
                foreach (var sim in sims)
                {
                    if (UIDtoClient.ContainsKey(sim.PersistID))
                    {
                        var client = UIDtoClient[sim.PersistID];
                        vm.SignalChatEvent(new VMChatEvent(0, VMChatEventType.Generic, "Found and banned " + name + ", with IP " + client.RemoteIP + "."));
                        BanIP(client.RemoteIP);
                    }
                }
            }
        }

        public void KickUser(VM vm, string name)
        {
            var sims = vm.Entities.Where(x => x is VMAvatar && x.ToString().ToLowerInvariant().Trim(' ') == name.ToLowerInvariant().Trim(' '));
            lock (ClientToUID)
            {
                foreach (var sim in sims)
                {
                    if (UIDtoClient.ContainsKey(sim.PersistID))
                    {
                        var client = UIDtoClient[sim.PersistID];
                        new Thread(() =>
                        {
                            client.Disconnect();
                        }).Start();
                    }
                }
            }
        }

        public void BanIP(string ip)
        {
            var cleanIP = ip.Trim(' ').ToLowerInvariant();
            var badClients = new List<NetworkClient>();
            lock (listener.Clients)
            {
                foreach (var client in listener.Clients)
                {
                    if (client.RemoteIP.ToLowerInvariant() == cleanIP) {
                        SendGenericMessage(client, "Yikes", "You have just been banned from this sandbox server!");
                        badClients.Add(client);
                    }
                }
            }
            foreach (var client in badClients) {
                new Thread(() =>
                {
                    client.Disconnect();
                }).Start();
            }
            SandboxBans.Add(cleanIP);
        }

        public override string GetUserIP(uint uid)
        {
            lock (UIDtoClient)
            {
                NetworkClient client = null;
                UIDtoClient.TryGetValue(uid, out client);
                if (client == null) return "unknown";
                return client.RemoteIP;
            }
        }
    }
}
