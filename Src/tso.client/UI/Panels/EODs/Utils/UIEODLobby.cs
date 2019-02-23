﻿using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.SimAntics;
using FSO.SimAntics.NetPlay.EODs.Utils;
using System;
using System.Collections.Generic;

namespace FSO.Client.UI.Panels.EODs.Utils
{
    public class UIEODLobby : UIContainer
    {
        int NumPlayers;
        UIEOD EOD;

        List<UIEODLobbyPlayer> PlayerUI;
        Func<UIEODLobbyPlayer, VMAvatar, string> CaptionFunction;

        public UIEODLobby(UIEOD eod, int numPlayers)
        {
            EOD = eod;
            NumPlayers = numPlayers;
            PlayerUI = new List<UIEODLobbyPlayer>();
        }

        public UIEODLobby WithCaptionProvider(Func<UIEODLobbyPlayer, VMAvatar, string> func)
        {
            CaptionFunction = func;
            return this;
        }

        public UIEODLobby WithPlayerUI(UIEODLobbyPlayer player)
        {
            PlayerUI.Add(player);
            return this;
        }

        public UIVMPersonButton GetPlayerButton(int playerIndex)
        {
            if (playerIndex != -1)
                return PlayerUI[playerIndex].PersonButton;
            return null;
        }

        public UIVMPersonButton GetAvatarButton(short objectID, bool small)
        {
            var avatar = (VMAvatar)EOD.Controller.Lot.vm.GetObjectById((short)objectID);
            if (avatar != null)
                return new UIVMPersonButton((VMAvatar)avatar, EOD.Controller.Lot.vm, small);
            return null;
        }

        public void UpdatePlayers(string evt, string msg)
        {
            var players = EODLobby.ParsePlayers(msg);
            if (players.Length != NumPlayers) { return; }

            for(var i=0; i < PlayerUI.Count; i++)
            {
                var ui = PlayerUI[i];
                var playerId = players[ui.Slot];

                var avatar = (VMAvatar)EOD.Controller.Lot.vm.GetObjectById((short)playerId);

                if (avatar == null)
                {
                    //No player
                    RemovePlayerUI(ui);
                }
                else
                {
                    UpdatePlayerUI(ui, avatar);
                }
            }
        }

        void UpdatePlayerUI(UIEODLobbyPlayer player, VMAvatar avatar)
        {
            if (player.PersonButton != null)
            {
                Remove(player.PersonButton);
                player.PersonButton = null;
            }

            player.PersonButton = new UIVMPersonButton((VMAvatar)avatar, EOD.Controller.Lot.vm, true)
            {
                Position = player.Image.Position + new Microsoft.Xna.Framework.Vector2(2, 2)
            };
            Add(player.PersonButton);

            if (CaptionFunction != null)
            {
                player.Label.Caption = CaptionFunction(player, avatar);
            }
        }

        void RemovePlayerUI(UIEODLobbyPlayer player)
        {
            if(player.PersonButton != null)
            {
                Remove(player.PersonButton);
                player.PersonButton = null;
            }

            if (CaptionFunction != null)
            {
                player.Label.Caption = CaptionFunction(player, null);
            }
        }
    }

    public class UIEODLobbyPlayer
    {
        public int Slot { get; internal set; }

        public UIImage Image { get; internal set; }
        public UILabel Label { get; internal set; }
        public UIVMPersonButton PersonButton;

        public UIEODLobbyPlayer(int slot, UIImage background, UILabel label)
        {
            Slot = slot;
            Image = background;
            Label = label;
        }
    }
}