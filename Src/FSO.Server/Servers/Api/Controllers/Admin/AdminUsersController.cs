using FSO.Server.Database.DA;
using Nancy;
using System;
using Nancy.Security;
using FSO.Server.Common;
using FSO.Server.Servers.Api.JsonWebToken;
using FSO.Server.Database.DA.Users;
using Nancy.ModelBinding;

namespace FSO.Server.Servers.Api.Controllers
{
    /// <summary>
    /// Provides administration APIs for server setup
    /// </summary>
    public class AdminUsersController : NancyModule
    {
        private readonly IDAFactory _daFactory;

        public AdminUsersController(IDAFactory daFactory, JWTFactory jwt) : base("/admin")
        {
            JWTTokenAuthentication.Enable(this, jwt);

            _daFactory = daFactory;

            After.AddItemToEndOfPipeline(x =>
            {
                x.Response.WithHeader("Access-Control-Allow-Origin", "*");
            });

            //Get information about me, useful for the admin user interface to disable UI based on who you login as
            Get["/users/current"] = _ =>
            {
                this.RequiresAuthentication();
                var user = (JWTUserIdentity)Context.CurrentUser;

                using (var da = daFactory.Get)
                {
                    var userModel = da.Users.GetById(user.UserID);
                    if (userModel == null)
                    { throw new Exception("Unable to find user"); }
                    return Response.AsJson(userModel);
                }
            };

            //Get the attributes of a specific user
            Get["/users/{id}"] = parameters =>
            {
                this.DemandModerator();

                using (var da = daFactory.Get)
                {
                    var userModel = da.Users.GetById((uint)parameters.id);
                    if (userModel == null)
                    { throw new Exception("Unable to find user"); }
                    return Response.AsJson(userModel);
                }
            };

            //List users
            Get["/users"] = _ =>
            {
                this.DemandModerator();
                using (var da = daFactory.Get)
                {
                    var offset = Request.Query["offset"];
                    var limit = Request.Query["limit"];

                    if (offset == null)
                    { offset = 0; }
                    if (limit == null)
                    { limit = 20; }

                    if (limit > 100)
                    {
                        limit = 100;
                    }

                    var result = da.Users.All((int)offset, (int)limit);
                    return Response.AsPagedList(result);
                }
            };

            //Create a new user
            Post["/users"] = x =>
            {
                this.DemandModerator();
                var user = this.Bind<UserCreateModel>();

                if (user.is_admin)
                {
                    //I need admin claim to do this
                    this.DemandAdmin();
                }

                using (var da = daFactory.Get)
                {
                    var userModel = new User
                    {
                        username = user.username,
                        email = user.email,
                        is_admin = user.is_admin,
                        is_moderator = user.is_moderator,
                        user_state = UserState.valid,
                        register_date = Epoch.Now,
                        is_banned = false
                    };

                    var userId = da.Users.Create(userModel);

                    userModel = da.Users.GetById(userId);
                    if (userModel == null)
                    { throw new Exception("Unable to find user"); }
                    return Response.AsJson(userModel);
                }

                return null;
            };
        }
    }

    class UserCreateModel
    {
        public string username;
        public string email;
        public string password;
        public bool is_admin;
        public bool is_moderator;
    }


}
