﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Server.Common
{
    public interface IPacketLogger
    {
        void OnPacket(Packet packet);
    }

    public class Packet
    {
        public PacketType Type;
        public ushort SubType;

        public byte[] Data;
        public PacketDirection Direction;
    }

    public enum PacketType
    {
        ARIES,
        VOLTRON,
        ELECTRON
    }

    public enum PacketDirection
    {
        OUTPUT,
        INPUT
    }
}
