using Nancy;
using System;

namespace FSO.Server.Servers.Api.JsonWebToken
{
    public class JWTTokenAuthentication
    {
        const string Scheme = "bearer";

        public static void Enable(INancyModule module, JWTFactory factory)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }

            module.Before.AddItemToStartOfPipeline(GetCredentialRetrievalHook(factory));
        }

        static Func<NancyContext, Response> GetCredentialRetrievalHook(JWTFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("configuration");
            }

            return context =>
            {
                RetrieveCredentials(context, factory);
                return null;
            };
        }

        static void RetrieveCredentials(NancyContext context, JWTFactory factory)
        {
            var token = ExtractTokenFromHeader(context.Request);
            if (token == null)
            {
                return;
            }

            try {
                var user = factory.DecodeToken(token);
                if (user != null) {
                    var identity = new JWTUserIdentity()
                    {
                        UserID = user.UserID,
                        UserName = user.UserName,
                        Claims = user.Claims
                    };
                    context.CurrentUser = identity;
                }
            }catch(Exception ex){
                //Expired
            }
        }

        static string ExtractTokenFromHeader(Request request)
        {
            var authorization = request.Headers.Authorization;

            if (string.IsNullOrEmpty(authorization))
            {
                //City selector puts it in a cookie
                if (request.Cookies.ContainsKey("fso"))
                {
                    return request.Cookies["fso"];
                }
                return null;
            }

            if (!authorization.StartsWith(Scheme))
            {
                return null;
            }

            try
            {
                var encodedToken = authorization.Substring(Scheme.Length).Trim();
                return String.IsNullOrWhiteSpace(encodedToken) ? null : encodedToken;
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
