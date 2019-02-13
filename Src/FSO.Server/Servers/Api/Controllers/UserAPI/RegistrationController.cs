using FSO.Server.Common;
using FSO.Server.Database.DA;
using FSO.Server.Database.DA.Users;
using FSO.Server.Servers.Api.JsonWebToken;
using Nancy;
using Nancy.ModelBinding;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FSO.Server.Servers.Api.Controllers.UserAPI
{
    public class RegistrationController : NancyModule
    {
        private IDAFactory DAFactory;
        private const int REGISTER_THROTTLE_SECS = 60*60*24;

        /// <summary>
        /// Alphanumeric (lowercase), no whitespace or special chars, cannot start with an underscore.
        /// </summary>
        private static Regex USERNAME_VALIDATION = new Regex("^([a-z0-9]){1}([a-z0-9_]){2,23}$");

        public RegistrationController(IDAFactory daFactory, JWTFactory jwt, ApiServerConfiguration config) : base("/userapi/registration")
        {
            JWTTokenAuthentication.Enable(this, jwt);

            DAFactory = daFactory;

            After.AddItemToEndOfPipeline(x =>
            {
                x.Response.WithHeader("Access-Control-Allow-Origin", "*");
            });

            //Create a new user
            Post["/"] = x =>
            {
                var user = this.Bind<RegistrationModel>();
                var tryIP = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (tryIP != null) tryIP = tryIP.Substring(tryIP.LastIndexOf(',') + 1).Trim();
                var ip = tryIP ?? Request.UserHostAddress;

                user.username = user.username ?? "";
                user.username = user.username.ToLowerInvariant();
                user.email = user.email ?? "";
                user.key = user.key ?? "";
                string failReason = null;
                if (user.username.Length < 3) failReason = "user_short";
                else if (user.username.Length > 24) failReason = "user_long";
                else if (!USERNAME_VALIDATION.IsMatch(user.username ?? "")) failReason = "user_invalid";
                else if ((user.password?.Length ?? 0) == 0) failReason = "pass_required";

                if (failReason != null)
                {
                    return Response.AsJson(new RegistrationError()
                    {
                        error = "bad_request",
                        error_description = failReason
                    });
                }

                bool isAdmin = false;
                if (config.Regkey != null && config.Regkey != user.key)
                {
                    return Response.AsJson(new RegistrationError()
                    {
                        error = "key_wrong",
                        error_description = failReason
                    });
                }

                var passhash = PasswordHasher.Hash(user.password);

                using (var da = daFactory.Get)
                {
                    //has this ip been banned?
                    var ban = da.Bans.GetByIP(ip);
                    if (ban != null)
                    {
                        return Response.AsJson(new RegistrationError()
                        {
                            error = "registration_failed",
                            error_description = "ip_banned"
                        });
                    }

                    //has this user registered a new account too soon after their last?
                    var now = Epoch.Now;
                    var prev = da.Users.GetByRegisterIP(ip);
                    if (now - (prev.FirstOrDefault()?.register_date ?? 0) < REGISTER_THROTTLE_SECS)
                    {
                        //cannot create a new account this soon.
                        return Response.AsJson(new RegistrationError()
                        {
                            error = "registration_failed",
                            error_description = "registrations_too_frequent"
                        });
                    }

                    //TODO: is this ip banned?

                    var userModel = new User
                    {
                        username = user.username,
                        email = user.email,
                        is_admin = isAdmin,
                        is_moderator = isAdmin,
                        user_state = UserState.valid,
                        register_date = now,
                        is_banned = false,
                        register_ip = ip,
                        last_ip = ip
                    };

                    var authSettings = new UserAuthenticate
                    {
                        scheme_class = passhash.scheme,
                        data = passhash.data
                    };

                    try
                    {
                        var userId = da.Users.Create(userModel);
                        authSettings.user_id = userId;
                        da.Users.CreateAuth(authSettings);

                        userModel = da.Users.GetById(userId);
                        if (userModel == null) { throw new Exception("Unable to find user"); }
                        return Response.AsJson(userModel);
                    } catch (Exception)
                    {
                        return Response.AsJson(new RegistrationError()
                        {
                            error = "registration_failed",
                            error_description = "user_exists"
                        });
                    }

                }
            };
        }
    }

    class RegistrationError
    {
        public string error_description { get; set; }
        public string error { get; set; }
    }

    class RegistrationModel
    {
        public string username;
        public string email;
        public string password;
        public string key;
    }
}
