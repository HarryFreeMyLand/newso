using Nancy;
using Nancy.ErrorHandling;

namespace FSO.Server.Servers.Api.Controllers
{
    public class StatusCodeHandler : IStatusCodeHandler
    {
        readonly IRootPathProvider _rootPathProvider;

        public StatusCodeHandler(IRootPathProvider rootPathProvider)
        {
            _rootPathProvider = rootPathProvider;
        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return statusCode == HttpStatusCode.NotFound;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            context.Response.Contents = stream =>
            {
                 
            };
        }
    }
}
