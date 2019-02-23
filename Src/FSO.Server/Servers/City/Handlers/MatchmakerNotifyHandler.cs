﻿using FSO.Server.Framework.Gluon;
using FSO.Server.Protocol.Gluon.Packets;
using FSO.Server.Servers.City.Domain;

namespace FSO.Server.Servers.City.Handlers
{
    public class MatchmakerNotifyHandler
    {
        JobMatchmaker Matchmaker;
        CityServerContext Context;

        public MatchmakerNotifyHandler(JobMatchmaker mm, CityServerContext context)
        {
            Context = context;
            Matchmaker = mm;
        }

        public async void Handle(IGluonSession session, MatchmakerNotify packet)
        {
            switch (packet.Mode)
            {
                case MatchmakerNotifyType.RemoveAvatar:
                    Matchmaker.RemoveAvatar(packet.LotID, packet.AvatarID);
                    break;
            }
            
        }
    }
}