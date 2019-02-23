﻿using FSO.SimAntics.Engine.TSOGlobalLink.Model;
using FSO.SimAntics.NetPlay.EODs.Model;
using FSO.SimAntics.NetPlay.Model.Commands;
using System.Linq;

namespace FSO.SimAntics.NetPlay.EODs.Handlers
{
    public class VMEODRackPlugin : VMAbstractEODRackPlugin
    {
        public VMEODRackPlugin(VMEODServer server) : base(server)
        {
            PlaintextHandlers["rack_try_outfit_on"] = TryOutfitOn;
            PlaintextHandlers["rack_purchase"] = Purchase;
        }

            void Purchase(string evt, string data, VMEODClient client)
        {
            var split = data.Split(',');
            if (split.Length != 2) { return; }

            if (!uint.TryParse(split[0], out var outfitId))
            {
                return;
            }

            if (!bool.TryParse(split[1], out var putOnNow))
            {
                return;
            }

            var VM = client.vm;


            GetOutfit(VM, outfitId, outfit =>
            {
                if (outfit == null) { return; }


                //Make sure we don't already have this outfit, can't have an outfit twice
                VM.GlobalLink.GetOutfits(VM, VMGLOutfitOwner.AVATAR, Controller.Avatar.PersistID, avatarOutfits =>
                {
                    if(avatarOutfits.FirstOrDefault(x => x.asset_id == outfit.asset_id) != null){
                        //I already have this outfit
                        return;
                    }

                    //Take payment
                    VM.GlobalLink.PerformTransaction(VM, false, client.Avatar.PersistID, Server.Object.PersistID, outfit.sale_price,

                    (bool success, int transferAmount, uint uid1, uint budget1, uint uid2, uint budget2) =>
                    {
                        if (success)
                        {
                            //Transfer outfit to my avatar
                            VM.GlobalLink.PurchaseOutfit(VM, outfit.outfit_id, Server.Object.PersistID, client.Avatar.PersistID, purchaseSuccess => {
                                    if (purchaseSuccess && putOnNow)
                                    {
                                        PutOnNow(outfit, client);
                                    }

                                    BroadcastOutfits(VM, true);
                                });
                        }
                    });


                });
            });
        }

            void PutOnNow(VMGLOutfit outfit, VMEODClient client)
        {
            var slot = GetSuitSlot(false);
            client.vm.SendCommand(new VMNetSetOutfitCmd
            {
                UID = client.Avatar.PersistID,
                Scope = slot,
                Outfit = outfit.asset_id
            });
            client.SendOBJEvent(new VMEODEvent((short)VMEODRackEvent.PutOnNow, (short)RackType));
        }

            void TryOutfitOn(string evt, string data, VMEODClient client)
        {
            if (!uint.TryParse(data, out var outfitId))
            {
                return;
            }

            GetOutfit(client.vm, outfitId, outfit => {
                if (outfit == null) { return; }

                var slot = GetSuitSlot(true);

                //store the outfit under dynamic costume
                client.vm.SendCommand(new VMNetSetOutfitCmd {
                    UID = client.Avatar.PersistID,
                    Scope = slot,
                    Outfit = outfit.asset_id
                });
                //3 uses dynamic costume, by using this we avoid updating default outfits without a good reason to
                client.SendOBJEvent(new VMEODEvent((short)VMEODRackEvent.TryOnOutfit, (short)RackType));
            });
        }

        public override void OnDisconnection(VMEODClient client)
        {
            client.SendOBJEvent(new VMEODEvent((short)VMEODRackEvent.PutClothesBack, (short)RackType));
            base.OnDisconnection(client);
        }
    }
}
