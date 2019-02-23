using FSO.Server.Database.DA;
using FSO.Server.Protocol.Gluon.Model;
using FSO.Server.Servers.Api.JsonWebToken;
using Nancy;
using Nancy.ModelBinding;

namespace FSO.Server.Servers.Api.Controllers.Admin
{
    public class AdminShardOpController : NancyModule
    {
        IDAFactory DAFactory;
        ApiServer Server;

        public AdminShardOpController(IDAFactory daFactory, JWTFactory jwt, ApiServer server) : base("/admin/shards")
        {
            JWTTokenAuthentication.Enable(this, jwt);
            
            DAFactory = daFactory;
            Server = server;

            After.AddItemToEndOfPipeline(x =>
            {
                x.Response.WithHeader("Access-Control-Allow-Origin", "*");
            });

            Post["/shutdown"] = _ =>
            {
                this.DemandAdmin();
                var shutdown = this.Bind<ShutdownModel>();

                var type = ShutdownType.SHUTDOWN;
                if (shutdown.update) type = ShutdownType.UPDATE;
                else if (shutdown.restart) type = ShutdownType.RESTART;

                //JWTUserIdentity user = (JWTUserIdentity)this.Context.CurrentUser;
                Server.RequestShutdown((uint)shutdown.timeout, type);

                return Response.AsJson(true);
            };

            Post["/announce"] = _ =>
            {
                this.DemandModerator();
                var announce = this.Bind<AnnouncementModel>();

                Server.BroadcastMessage(announce.sender, announce.subject, announce.message);

                return Response.AsJson(true);
            };
        }
    }

    public class AnnouncementModel
    {
        public string sender;
        public string subject;
        public string message;
        public int[] shard_ids;
    }

    public class ShutdownModel
    {
        public int timeout;
        public bool restart;
        public bool update;
        public int[] shard_ids;
    }
}
