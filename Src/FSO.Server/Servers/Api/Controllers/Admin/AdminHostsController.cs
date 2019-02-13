using FSO.Server.Database.DA;
using FSO.Server.Domain;
using FSO.Server.Servers.Api.JsonWebToken;
using Nancy;
using System.Linq;

namespace FSO.Server.Servers.Api.Controllers.Admin
{
    public class AdminHostsController : NancyModule
    {
        public AdminHostsController(IDAFactory daFactory, JWTFactory jwt, IGluonHostPool hostPool) : base("/admin")
        {
            JWTTokenAuthentication.Enable(this, jwt);

            Get["/hosts"] = _ =>
            {
                this.DemandAdmin();
                var hosts = hostPool.GetAll();

                return Response.AsJson(hosts.Select(x => new {
                    role = x.Role,
                    call_sign = x.CallSign,
                    internal_host = x.InternalHost,
                    public_host = x.PublicHost,
                    connected = x.Connected,
                    time_boot = x.BootTime
                }));
            };
        }
    }
}