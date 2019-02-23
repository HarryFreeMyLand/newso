using FSO.Client.UI.Panels;
using FSO.Common.DataService;
using FSO.Common.DataService.Model;
using FSO.Common.Enum;
using System;

namespace FSO.Client.Controllers
{
    public class GizmoController : IDisposable
    {
        UIGizmo Gizmo;
        Network.Network Network;
        IClientDataService DataService;

        public GizmoController(UIGizmo view, Network.Network network, IClientDataService dataService)
        {
            Gizmo = view;
            Network = network;
            DataService = dataService;

            Initialize();
        }

        void Initialize()
        {
            DataService.Get<Avatar>(Network.MyCharacter).ContinueWith(x =>
            {
                if (!x.IsFaulted){
                    Gizmo.CurrentAvatar.Value = x.Result;
                    FSO.UI.Model.DiscordRpcEngine.SendFSOPresence(x.Result.Avatar_Name, null, 0, 0, 0, 0, x.Result.Avatar_PrivacyMode > 0);
                }
            });
        }

        public void Dispose()
        {
            try {
                Gizmo.CurrentAvatar.Value = null;
            }catch(Exception ex){
            }
        }

        public void RequestFilter(LotCategory cat)
        {
            if (Gizmo.CurrentAvatar != null && Gizmo.CurrentAvatar.Value != null)
            {
                Gizmo.CurrentAvatar.Value.Avatar_Top100ListFilter.Top100ListFilter_Top100ListID = (uint)cat;
                DataService.Sync(Gizmo.CurrentAvatar.Value, new string[] { "Avatar_Top100ListFilter.Top100ListFilter_Top100ListID" });
            }
        }

        public void ClearFilter()
        {
            Gizmo.FilterList = System.Collections.Immutable.ImmutableList<uint>.Empty;
        }
    }
}
