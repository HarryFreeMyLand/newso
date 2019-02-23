using FSO.Client.Model;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common.DatabaseService;
using FSO.Common.DatabaseService.Model;
using FSO.Common.DataService;
using FSO.Common.Domain.Shards;
using FSO.Common.Utils;
using FSO.Server.Clients;
using FSO.Server.Clients.Framework;
using FSO.Server.DataService.Model;
using FSO.Server.Protocol.Aries.Packets;
using FSO.Server.Protocol.CitySelector;
using FSO.Server.Protocol.Electron.Packets;
using FSO.Server.Protocol.Voltron.Packets;
using Ninject;
using System;
using System.Collections.Generic;

namespace FSO.Client.Regulators
{
    public class CityConnectionRegulator : AbstractRegulator, IAriesMessageSubscriber, IAriesEventSubscriber
    {
        public AriesClient Client { get; internal set; }
        public CityConnectionMode Mode { get; internal set; } = CityConnectionMode.NORMAL;

        CityClient CityApi;
        ShardSelectorServletResponse ShardSelectResponse;
        public ShardSelectorServletRequest CurrentShard;
        IDatabaseService DB;
        IClientDataService DataService;
        IShardsDomain Shards;

        public CityConnectionRegulator(CityClient cityApi, [Named("City")] AriesClient cityClient, IDatabaseService db, IClientDataService ds, IKernel kernel, IShardsDomain shards)
        {
            CityApi = cityApi;
            Client = cityClient;
            Client.AddSubscriber(this);
            DB = db;
            DataService = ds;
            Shards = shards;

            AddState("Disconnected")
                .Default()
                .Transition()
                .OnData(typeof(ShardSelectorServletRequest))
                .TransitionTo("SelectCity");

            AddState("SelectCity")
                .OnlyTransitionFrom("Disconnected", "Reconnecting");

            AddState("ConnectToCitySelector")
                .OnData(typeof(ShardSelectorServletResponse))
                .TransitionTo("CitySelected")
                .OnlyTransitionFrom("SelectCity");

            AddState("CitySelected")
                .OnData(typeof(ShardSelectorServletResponse))
                .TransitionTo("OpenSocket")
                .OnlyTransitionFrom("ConnectToCitySelector");

            AddState("OpenSocket")
                .OnData(typeof(AriesConnected)).TransitionTo("SocketOpen")
                .OnData(typeof(AriesDisconnected)).TransitionTo("UnexpectedDisconnect")
                .OnlyTransitionFrom("CitySelected");

            AddState("SocketOpen")
                .OnData(typeof(RequestClientSession)).TransitionTo("RequestClientSession")
                .OnData(typeof(AriesDisconnected)).TransitionTo("UnexpectedDisconnect")
                .OnlyTransitionFrom("OpenSocket");

            AddState("RequestClientSession")
                .OnData(typeof(HostOnlinePDU)).TransitionTo("HostOnline")
                .OnData(typeof(AriesDisconnected)).TransitionTo("UnexpectedDisconnect")
                .OnlyTransitionFrom("SocketOpen");

            AddState("HostOnline").OnlyTransitionFrom("RequestClientSession");
            AddState("PartiallyConnected")
                .OnData(typeof(AriesDisconnected)).TransitionTo("UnexpectedDisconnect")
                .OnData(typeof(ShardSelectorServletRequest)).TransitionTo("CompletePartialConnection")
                .OnlyTransitionFrom("HostOnline");

            AddState("CompletePartialConnection").OnlyTransitionFrom("PartiallyConnected");
            AddState("AskForAvatarData")
                .OnData(typeof(LoadAvatarByIDResponse)).TransitionTo("ReceivedAvatarData")
                .OnlyTransitionFrom("PartiallyConnected", "CompletePartialConnection");
            AddState("ReceivedAvatarData").OnlyTransitionFrom("AskForAvatarData");
            AddState("AskForCharacterData").OnlyTransitionFrom("ReceivedAvatarData");
            AddState("ReceivedCharacterData").OnlyTransitionFrom("AskForCharacterData");
            
            AddState("Connected")
                .OnData(typeof(AriesDisconnected)).TransitionTo("UnexpectedDisconnect")
                .OnlyTransitionFrom("ReceivedCharacterData");

            AddState("UnexpectedDisconnect");

            AddState("Disconnect")
                .OnData(typeof(AriesDisconnected))
                .TransitionTo("Disconnected");

            AddState("Reconnect")
                .OnData(typeof(AriesDisconnected))
                .TransitionTo("Reconnecting");

            AddState("Reconnecting")
                .OnData(typeof(ShardSelectorServletRequest))
                .TransitionTo("SelectCity")
                .OnlyTransitionFrom("Reconnect");

            GameThread.SetInterval(() =>
            {
                if (Client.IsConnected)
                {
                    Client.Write(new KeepAlive());
                }
            }, 10000); //keep alive every 10 seconds. prevents disconnection by aggressive NAT.
        }

        public void Connect(CityConnectionMode mode, ShardSelectorServletRequest shard)
        {
            if(shard.ShardName == null && CurrentShard != null)
            {
                shard.ShardName = CurrentShard.ShardName;
            }
            Mode = mode;
            if (CurrentState.Name != "Disconnected")
            {
                CurrentShard = shard;
                AsyncTransition("Reconnect");
            }
            else
            {
                AsyncProcessMessage(shard);
            }
        }

        public void Disconnect(){
            AsyncTransition("Disconnect");
        }

        protected override void OnAfterTransition(RegulatorState oldState, RegulatorState newState, object data)
        {
        }

        protected override void OnBeforeTransition(RegulatorState oldState, RegulatorState newState, object data)
        {
            switch (newState.Name)
            {
                case "SelectCity":
                    //TODO: Do this on logout / disconnect rather than on connect
                    ResetGame();

                    var shard = data as ShardSelectorServletRequest;
                    if (shard == null)
                    {
                        ThrowErrorAndReset(new Exception("Unknown parameter"));
                    }

                    AsyncTransition("ConnectToCitySelector", shard);
                    break;

                case "ConnectToCitySelector":
                    shard = data as ShardSelectorServletRequest;
                    CurrentShard = shard;
                    ShardSelectResponse = CityApi.ShardSelectorServlet(shard);
                    AsyncProcessMessage(ShardSelectResponse);
                    break;

                case "CitySelected":
                    AsyncProcessMessage(data);
                    break;

                case "OpenSocket":
                    var settings = data as ShardSelectorServletResponse;
                    if (settings == null){
                        ThrowErrorAndReset(new Exception("Unknown parameter"));
                    }else{
                        //101 is plain
                        Client.Connect(settings.Address + "101");
                    }
                    break;

                case "SocketOpen":
                    break;

                case "RequestClientSession":
                    Client.Write(new RequestClientSessionResponse {
                        Password = ShardSelectResponse.Ticket,
                        User = ShardSelectResponse.PlayerID.ToString()
                    });
                    break;

                case "HostOnline":
                    ((ClientShards)Shards).CurrentShard = Shards.GetByName(CurrentShard.ShardName).Id;
                    
                    Client.Write(
                        new ClientOnlinePDU {
                        },
                        new SetIgnoreListPDU {
                            PlayerIds = new List<uint>()
                        },
                        new SetInvinciblePDU{
                            Action = 0
                        }
                    );
                    AsyncTransition("PartiallyConnected");
                    break;

                case "PartiallyConnected":
                    if(Mode == CityConnectionMode.NORMAL){
                        AsyncTransition("AskForAvatarData");
                    }
                    break;

                case "CompletePartialConnection":
                    var shardRequest = (ShardSelectorServletRequest)data;
                    if (shardRequest.ShardName != CurrentShard.ShardName)
                    {
                        //Should never get into this state
                        throw new Exception("You cant complete a partial connection for a different city");
                    }
                    CurrentShard = shardRequest;
                    AsyncTransition("AskForAvatarData");
                    break;

                case "AskForAvatarData":
                    DB.LoadAvatarById(new LoadAvatarByIDRequest
                    {
                        AvatarId = uint.Parse(CurrentShard.AvatarID)
                    }).ContinueWith(x =>
                    {
                        if (x.IsFaulted) {
                            ThrowErrorAndReset(new Exception("Failed to load avatar from db"));
                        } else{
                            AsyncProcessMessage(x.Result);
                        }
                    });
                    break;

                case "ReceivedAvatarData":
                    AsyncTransition("AskForCharacterData");
                    break;

                case "AskForCharacterData":
                    DataService.Request(MaskedStruct.MyAvatar, uint.Parse(CurrentShard.AvatarID)).ContinueWith(x =>
                    {
                        if (x.IsFaulted)
                        {
                            ThrowErrorAndReset(new Exception("Failed to load character from db"));
                        }
                        else
                        {
                            AsyncTransition("ReceivedCharacterData");
                        }
                    });
                    break;

                case "ReceivedCharacterData":
                    //For now, we will call this connected
                    AsyncTransition("Connected");
                    break;

                case "Connected":
                    break;

                case "UnexpectedDisconnect":
                    FSOFacade.Controller.FatalNetworkError(23);
                    break;

                case "Disconnect":
                    ShardSelectResponse = null;
                    if (Client.IsConnected)
                    {
                        Client.Write(new ClientByePDU());
                        Client.Disconnect();
                    }
                    else
                    {
                        AsyncTransition("Disconnected");
                    }
                    break;

                case "Reconnect":
                    ShardSelectResponse = null;
                    if (Client.IsConnected)
                    {
                        Client.Write(new ClientByePDU());
                        Client.Disconnect();
                    }
                    else
                    {
                        AsyncTransition("Reconnecting");
                    }
                    break;
                case "Reconnecting":
                    AsyncProcessMessage(CurrentShard);
                    break;
                case "Disconnected":
                    ((ClientShards)Shards).CurrentShard = null;
                    break;
            }
        }

        public void ResetGame()
        {
            UserReference.ResetCache();
        }


        public void MessageReceived(AriesClient client, object message)
        {

            if (message is RequestClientSession || 
                message is HostOnlinePDU){
                AsyncProcessMessage(message);
            }
            else if (message is AnnouncementMsgPDU)
            {
                GameThread.InUpdate(() => {
                    var msg = (AnnouncementMsgPDU)message;
                    UIAlert alert = null;
                    alert = UIScreen.GlobalShowAlert(new UIAlertOptions()
                    {
                        Title = GameFacade.Strings.GetString("195", "30") + GameFacade.CurrentCityName,
                        Message = GameFacade.Strings.GetString("195", "28") + msg.SenderID.Substring(2) + "\r\n"
                        + GameFacade.Strings.GetString("195", "29") + msg.Subject + "\r\n"
                        + msg.Message,
                        Buttons = UIAlertButton.Ok((btn) => UIScreen.RemoveDialog(alert)),
                        Alignment = TextAlignment.Left
                    }, true);
                });
            } else if (message is ChangeRoommateResponse)
            {

            }
        }

        public void SessionCreated(AriesClient client)
        {
            AsyncProcessMessage(new AriesConnected());
        }

        public void SessionOpened(AriesClient client)
        {

        }

        public void SessionClosed(AriesClient client)
        {
            AsyncProcessMessage(new AriesDisconnected());
        }

        public void SessionIdle(AriesClient client)
        {

        }

        public void InputClosed(AriesClient session)
        {
            AsyncProcessMessage(new AriesDisconnected());
        }
    }

    public enum CityConnectionMode
    {
        CAS,
        NORMAL
    }

    class AriesConnected {

    }

    class AriesDisconnected {

    }
}
