﻿using FSO.Server.Domain;
using FSO.Server.Framework.Aries;
using FSO.Server.Framework.Gluon;
using FSO.Server.Protocol.Aries.Packets;
using FSO.Server.Protocol.Gluon.Packets;
using FSO.Server.Protocol.Utils;

namespace FSO.Server.Servers.Shared.Handlers
{
    public class GluonAuthenticationHandler
    {
        IGluonHostPool HostPool;
        ISessions Sessions;
        string Secret;

        public GluonAuthenticationHandler(ISessions sessions, ServerConfiguration config, IGluonHostPool hostPool){
            Sessions = sessions;
            Secret = config.Secret;
            HostPool = hostPool;
        }

        public void Handle(IAriesSession session, RequestChallenge request)
        {
            var challenge = ChallengeResponse.GetChallenge();
            session.SetAttribute("challenge", challenge);
            session.SetAttribute("callSign", request.CallSign);
            session.SetAttribute("publicHost", request.PublicHost);
            session.SetAttribute("internalHost", request.InternalHost);

            session.Write(new RequestChallengeResponse {
                Challenge = challenge
            });
        }

        public void Handle(IAriesSession session, AnswerChallenge answer)
        {
            var challenge = session.GetAttribute("challenge") as string;
            if(challenge == null)
            {
                session.Close();
                return;
            }

            var myAnswer = ChallengeResponse.AnswerChallenge(challenge, Secret);
            if(myAnswer != answer.Answer)
            {
                session.Close();
                return;
            }

            //Trust established, good to go
            var newSession = Sessions.UpgradeSession<GluonSession>(session, x => {
                x.IsAuthenticated = true;
                x.CallSign = (string)session.GetAttribute("callSign");
                x.PublicHost = (string)session.GetAttribute("publicHost");
                x.InternalHost = (string)session.GetAttribute("internalHost");
            });
            newSession.Write(new AnswerAccepted());
        }

        public void Handle(IGluonSession session, HealthPing ping)
        {
            session.Write(new HealthPingResponse {
                CallId = ping.CallId,
                PoolHash = HostPool.PoolHash
            });
        }
    }
}
