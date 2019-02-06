using FSO.Client.Regulators;
using FSO.Client.UI.Screens;
using FSO.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Client.Controllers
{
    public class DisconnectController : IDisposable
    {
        TransitionScreen View;
        CityConnectionRegulator CityConnectionRegulator;
        LotConnectionRegulator LotConnectionRegulator;
        LoginRegulator LoginRegulator;

        int totalComplete = 0;
        int targetComplete = 2;
        Action<bool> onDisconnected;

        public DisconnectController(TransitionScreen view, CityConnectionRegulator cityRegulator, LotConnectionRegulator lotRegulator, LoginRegulator logRegulator, Network.Network network)
        {
            View = view;
            View.ShowProgress = false;

            network.LotClient.Disconnect();
            network.CityClient.Disconnect();
            CityConnectionRegulator = cityRegulator;
            CityConnectionRegulator.OnTransition += CityConnectionRegulator_OnTransition;
            LotConnectionRegulator = lotRegulator;
            LoginRegulator = logRegulator;
            LoginRegulator.OnError += LoginRegulator_OnError;
            LoginRegulator.OnTransition += LoginRegulator_OnTransition;
        }

        void LoginRegulator_OnTransition(string state, object data)
        {
            switch (state)
            {
                case "LoggedIn":
                    if (++totalComplete == targetComplete) onDisconnected(false);
                    break;
            }
        }

        void LoginRegulator_OnError(object data)
        {
            onDisconnected(true);
        }

        void CityConnectionRegulator_OnTransition(string state, object data)
        {
            switch (state)
            {
                case "Disconnect":
                    break;
                case "Disconnected":
                    if (++totalComplete == targetComplete) onDisconnected(false);
                    break;
            }
        }

        public void Disconnect(Action<bool> onDisconnected, bool forceLogin)
        {
            totalComplete = 0;
            this.onDisconnected = onDisconnected;

            if (forceLogin)
            {
                targetComplete = 1;
                LoginRegulator.Logout();
            }

            CityConnectionRegulator.Disconnect();
            LotConnectionRegulator.Disconnect();

            if (!forceLogin) LoginRegulator.AsyncTransition("AvatarData");
        }

        public void Dispose()
        {
            CityConnectionRegulator.OnTransition -= CityConnectionRegulator_OnTransition;
            LoginRegulator.OnTransition -= LoginRegulator_OnTransition;
            LoginRegulator.OnError -= LoginRegulator_OnError;
        }
    }
}
