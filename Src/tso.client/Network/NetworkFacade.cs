﻿/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using GonzoNet;
using ProtocolAbstractionLibraryD;
using FSO.Client.UI.Controls;
using FSO.SimAntics;

namespace FSO.Client.Network
{
    /// <summary>
    /// Handles access to all of the network systems, service clients, city server, login events etc.
    /// </summary>
    public class NetworkFacade
    {
        public static NetworkClient Client;

        /// <summary>
        /// Used for AES encryption.
        /// </summary>
        public static byte[] ClientNOnce;

        /// <summary>
        /// Handles the movement between network states
        /// </summary>
        public static NetworkController Controller;

        /// <summary>
        /// List of cities, this is requested from the service client during login
        /// </summary>
        public static List<CityInfo> Cities = new List<CityInfo>();

        /// <summary>
        /// List of my avatars, this is requested from the service client during login
        /// </summary>
        public static List<UISim> Avatars = new List<UISim>();

        /// <summary>
        /// List of avatars in current session (game) on a cityserver.
        /// </summary>
        public static List<UISim> AvatarsInSession = new List<UISim>();

        /// <summary>
        /// List of VMs in the current game session
        /// </summary>
        public static List<VM> VMs = new List<VM>();

        //// <summary>
        /// Difference between local UTC time and the server's UTC time
        /// </summary>
        //public static long ClockOffset = 0;
        /*public static DateTime ServerTime
        {
            get
            {
                var now = new DateTime(DateTime.UtcNow.Ticks + ClockOffset);
                return now;
            }
        }*/
        public static DateTime ServerTime = DateTime.Now;

        static NetworkFacade()
        {
            Client = new NetworkClient(GlobalSettings.Default.LoginServerIP, GlobalSettings.Default.LoginServerPort, 
                GonzoNet.Encryption.EncryptionMode.AESCrypto, true);
            Client.OnConnected += new OnConnectedDelegate(UIPacketSenders.SendLoginRequest);
            Controller = new NetworkController();
            Controller.Init(Client);

            RNGCryptoServiceProvider Random = new RNGCryptoServiceProvider();
            ClientNOnce = new byte[16];
            Random.GetNonZeroBytes(ClientNOnce);

            //PacketHandlers.Init();
            PacketHandlers.Register((byte)PacketType.LOGIN_NOTIFY, false, 0, new OnPacketReceive(Controller._OnLoginNotify));
            PacketHandlers.Register((byte)PacketType.LOGIN_FAILURE, false, 0, new OnPacketReceive(Controller._OnLoginFailure));
            PacketHandlers.Register((byte)PacketType.LOGIN_SUCCESS, true, 0, new OnPacketReceive(Controller._OnLoginSuccess));
            PacketHandlers.Register((byte)PacketType.INVALID_VERSION, false, 2, new OnPacketReceive(Controller._OnInvalidVersion));
            PacketHandlers.Register((byte)PacketType.CHARACTER_LIST, true, 0, new OnPacketReceive(Controller._OnCharacterList));
            PacketHandlers.Register((byte)PacketType.CITY_LIST, true, 0, new OnPacketReceive(Controller._OnCityList));
            PacketHandlers.Register((byte)PacketType.NEW_CITY_SERVER, true, 0, new OnPacketReceive(Controller._OnNewCity));
            PacketHandlers.Register((byte)PacketType.CITY_SERVER_OFFLINE, true, 0, new OnPacketReceive(Controller._OnCityServerOffline));
            PacketHandlers.Register((byte)PacketType.CHARACTER_CREATION_STATUS, true, 0, new OnPacketReceive(Controller._OnCharacterCreationProgress));
            PacketHandlers.Register((byte)PacketType.RETIRE_CHARACTER_STATUS, true, 0, new OnPacketReceive(Controller._OnRetireCharacterStatus));

            PacketHandlers.Register((byte)PacketType.LOGIN_NOTIFY_CITY, false, 0, new OnPacketReceive(Controller._OnLoginNotifyCity));
            PacketHandlers.Register((byte)PacketType.LOGIN_SUCCESS_CITY, true, 0, new OnPacketReceive(Controller._OnLoginSuccessCity));
            PacketHandlers.Register((byte)PacketType.LOGIN_FAILURE_CITY, false, 0, new OnPacketReceive(Controller._OnLoginFailureCity));
            PacketHandlers.Register((byte)PacketType.CHARACTER_CREATE_CITY, true, 0, new OnPacketReceive(Controller._OnCharacterCreationStatus));
            PacketHandlers.Register((byte)PacketType.REQUEST_CITY_TOKEN, true, 0, new OnPacketReceive(Controller._OnCityToken));
            PacketHandlers.Register((byte)PacketType.CITY_TOKEN, true, 0, new OnPacketReceive(Controller._OnCityTokenResponse));
            PacketHandlers.Register((byte)PacketType.PLAYER_JOINED_SESSION, true, 0, new OnPacketReceive(Controller._OnPlayerJoinedSession));
            PacketHandlers.Register((byte)PacketType.PLAYER_LEFT_SESSION, true, 0, new OnPacketReceive(Controller._OnPlayerLeftSession));
            PacketHandlers.Register((byte)PacketType.PLAYER_RECV_LETTER, true, 0, new OnPacketReceive(Controller._OnPlayerRecvdLetter));
            PacketHandlers.Register((byte)PacketType.PLAYER_ALREADY_ONLINE, true, 0, new OnPacketReceive(Controller._OnPlayerAlreadyOnline));
            PacketHandlers.Register((byte)PacketType.TIME_OF_DAY, true, 0, new OnPacketReceive(Controller._OnTimeOfDay));
            PacketHandlers.Register((byte)PacketType.LOT_COST, true, 0, new OnPacketReceive(Controller._OnLotCost));
            PacketHandlers.Register((byte)PacketType.LOT_UNBUILDABLE, true, 0, new OnPacketReceive(Controller._OnLotUnbuildable));
            PacketHandlers.Register((byte)PacketType.LOT_PURCHASE_FAILED, true, 0, new OnPacketReceive(Controller._OnLotBuyFailed));
            PacketHandlers.Register((byte)PacketType.LOT_PURCHASE_SUCCESSFUL, true, 0, new OnPacketReceive(Controller._OnLotPurchaseSuccessful));
            PacketHandlers.Register((byte)PacketType.LOT_NAME_TOO_LONG, true, 0, new OnPacketReceive(Controller._OnLotNameTooLong));

            PacketHandlers.Register((byte)PacketType.VM_PACKET, false, 0, new OnPacketReceive(UIPacketHandlers.OnVMPacket));
        }
    }
}
