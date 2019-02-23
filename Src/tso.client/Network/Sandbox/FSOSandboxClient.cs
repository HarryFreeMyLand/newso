using Mina.Core.Service;
using System;
using Mina.Core.Session;
using System.Net;
using System.Globalization;
using Mina.Transport.Socket;
using Mina.Core.Future;
using Mina.Filter.Codec;
using FSO.SimAntics.NetPlay.Model;
using FSO.Common.Utils;

namespace FSO.Client.Network.Sandbox
{
    public class FSOSandboxClient : IoHandler
    {
        IoConnector Connector;
        IoSession Session;

        public event Action<VMNetMessage> OnMessage;
        public event Action OnConnectComplete;

        public void Connect(string address)
        {
            Connect(CreateIPEndPoint(address));
        }

        public void Disconnect()
        {
            if (Session != null)
            {
                Session.Close(false);
            }
        }

        public void Connect(IPEndPoint target)
        {
            Connector = new AsyncSocketConnector
            {
                ConnectTimeoutInMillis = 10000,

                Handler = this
            };
            Connector.FilterChain.AddLast("protocol", new ProtocolCodecFilter(new FSOSandboxProtocol()));
            Connector.Connect(target, new Action<IoSession, IConnectFuture>(OnConnect));
        }

        void OnConnect(IoSession session, IConnectFuture future)
        {
            Session = session;
            GameThread.NextUpdate(x =>
            {
                OnConnectComplete();
            });
        }

        public void Write(params object[] packets)
        {
            if (Session != null)
            {
                Session.Write(packets);
            }
        }

        public bool IsConnected
        {
            get
            {
                return Session != null && Session.Connected;
            }
        }

        public static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length != 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (!IPAddress.TryParse(ep[0], out ip))
            {
                var addrs = Dns.GetHostEntry(ep[0]).AddressList;
                if (addrs.Length == 0)
                {
                    throw new FormatException("Invalid ip-address");
                }
                else ip = addrs[0];
            }

            int port;
            if (!int.TryParse(ep[1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }

        public void ExceptionCaught(IoSession session, Exception cause)
        {
        }

        public void InputClosed(IoSession session)
        {
        }

        public void MessageReceived(IoSession session, object message)
        {
            if (message is VMNetMessage nmsg)
            {
                OnMessage(nmsg);
            }
        }

        public void MessageSent(IoSession session, object message)
        {
        }

        public void SessionClosed(IoSession session)
        {
        }

        public void SessionCreated(IoSession session)
        {
        }

        public void SessionIdle(IoSession session, IdleStatus status)
        {
        }

        public void SessionOpened(IoSession session)
        {
        }
    }
}
