using Mina.Core.Service;
using Mina.Filter.Codec;
using Mina.Transport.Socket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Mina.Core.Session;
using FSO.SimAntics.NetPlay.Model;
using System.IO;
using FSO.Common.Utils;

namespace FSO.Client.Network.Sandbox
{
    public class FSOSandboxServer : IoHandler
    {
        //todo: persist id provider
        //right now just give each new connected user a higher persist id than the last
        public uint PersistID = 2;

        public event Action<VMNetClient, VMNetMessage> OnMessage;
        public event Action<VMNetClient> OnConnect;
        public event Action<VMNetClient> OnDisconnect;

        List<IoSession> _sessions = new List<IoSession>();

        public void ForceDisconnect(VMNetClient cli)
        {
            if (cli.NetHandle == null)
                return;
            ((IoSession)cli.NetHandle).Close(false);
        }

        public void ExceptionCaught(IoSession session, Exception cause)
        {
            session.Close(true);
        }

        public void SendMessage(VMNetClient cli, VMNetMessage msg)
        {
            if (cli.NetHandle == null)
                return;
            ((IoSession)cli.NetHandle).Write(msg);
        }

        public void Broadcast(VMNetMessage msg, HashSet<VMNetClient> ignore)
        {
            List<IoSession> cliClone;
            lock (_sessions)
                cliClone = new List<IoSession>(_sessions);
            foreach (var s in cliClone)
            {
                if (ignore.Contains(s.GetAttribute('c')))
                    continue;
                s.Write(msg);
            }
        }

        public void InputClosed(IoSession session)
        {
        }

        public void MessageReceived(IoSession session, object message)
        {
            if (message is VMNetMessage)
            {
                GameThread.NextUpdate(x =>
                {
                    var nmsg = (VMNetMessage)message;
                    var cli = (VMNetClient)session.GetAttribute('c');
                    if (cli.AvatarState == null)
                    {
                        //we're still waiting for the avatar state so the user can join
                        if (nmsg.Type == VMNetMessageType.AvatarData)
                        {
                            var state = new VMNetAvatarPersistState();
                            try
                            {
                                state.Deserialize(new BinaryReader(new MemoryStream(nmsg.Data)));
                            }
                            catch (Exception)
                            {
                                return;
                            }
                            cli.PersistID = state.PersistID;
                            cli.AvatarState = state;

                            OnConnect(cli);
                        }
                    }
                    else
                    {
                        OnMessage(cli, nmsg);
                    }
                });
            }
        }

        public void MessageSent(IoSession session, object message)
        {

        }

        public void SessionClosed(IoSession session)
        {
            var cli = (VMNetClient)session.GetAttribute('c');
            if (cli != null)
                GameThread.NextUpdate(x =>
                {
                    OnDisconnect(cli);
                });

            lock (_sessions)
                _sessions.Remove(session);
        }

        public void SessionCreated(IoSession session)
        {
            var cli = new VMNetClient()
            {
                PersistID = PersistID++,
                RemoteIP = session.RemoteEndPoint.ToString(),
                AvatarState = null,
            };
            cli.NetHandle = session;
            session.SetAttribute('c', cli);

            lock (_sessions)
                _sessions.Add(session);
        }

        public void SessionIdle(IoSession session, IdleStatus status)
        {

        }

        public void SessionOpened(IoSession session)
        {
        }

        AsyncSocketAcceptor _acceptor;

        public void Start(ushort port)
        {
            _acceptor = new AsyncSocketAcceptor();
            _acceptor.FilterChain.AddLast("protocol", new ProtocolCodecFilter(new FSOSandboxProtocol()));
            _acceptor.Handler = this;
            IPAddress.TryParse("0.0.0.0", out var ip);

            try
            {
                _acceptor.Bind(new IPEndPoint(ip, port));
            }
            catch
            {

            }
        }

        public void Shutdown()
        {
            _acceptor?.Dispose();
            lock (_sessions)
            {
                foreach (var s in _sessions)
                    s.Close(true);
            }
        }
    }
}
