using FSO.Server.Framework.Gluon;
using FSO.Server.Protocol.Gluon.Packets;
using FSO.Server.Servers.City.Domain;

namespace FSO.Server.Servers.City.Handlers
{
    public class LotServerLifecycleHandler
    {
        LotServerPicker PickingEngine;

        public LotServerLifecycleHandler(LotServerPicker pickingEngine)
        {
            PickingEngine = pickingEngine;
        }

        public void Handle(IGluonSession session, AdvertiseCapacity capacity)
        {
            PickingEngine.UpdateServerAdvertisement(session, capacity);
        }
    }
}
