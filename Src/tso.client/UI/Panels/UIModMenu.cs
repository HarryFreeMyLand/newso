﻿using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Server.Protocol.Electron.Model;

namespace FSO.Client.UI.Panels
{
    public class UIModMenu : UIDialog
    {
        UIImage Background;
        UIButton IPBanButton;

        public uint AvatarID;

        public UIModMenu() : base(UIDialogStyle.Tall | UIDialogStyle.Close, true)
        {
            SetSize(380, 300);
            Caption = "Do what to this user?";

            Position = new Microsoft.Xna.Framework.Vector2(
                (GlobalSettings.Default.GraphicsWidth / 2.0f) - (480/2),
                (GlobalSettings.Default.GraphicsHeight / 2.0f) - 150
            );

            IPBanButton = new UIButton
            {
                Caption = "IP Ban",
                Position = new Microsoft.Xna.Framework.Vector2(40, 50),
                Width = 300
            };
            IPBanButton.OnButtonClick += x =>
            {
                var controller = FindController<Controllers.CoreGameScreenController>();
                if (controller != null)
                    controller.ModRequest(AvatarID, ModerationRequestType.IPBAN_USER);
                UIScreen.RemoveDialog(this);
            };
            Add(IPBanButton);

            var BanButton = new UIButton
            {
                Caption = "Ban User",
                Position = new Microsoft.Xna.Framework.Vector2(40, 90),
                Width = 300
            };
            BanButton.OnButtonClick += x =>
            {
                var controller = FindController<Controllers.CoreGameScreenController>();
                if (controller != null)
                    controller.ModRequest(AvatarID, ModerationRequestType.BAN_USER);
                UIScreen.RemoveDialog(this);
            };
            Add(BanButton);

            var kickButton = new UIButton
            {
                Caption = "Kick Avatar",
                Position = new Microsoft.Xna.Framework.Vector2(40, 130),
                Width = 300
            };
            kickButton.OnButtonClick += x =>
            {
                var controller = FindController<Controllers.CoreGameScreenController>();
                if (controller != null)
                    controller.ModRequest(AvatarID, ModerationRequestType.KICK_USER);
                UIScreen.RemoveDialog(this);
            };
            Add(kickButton);

            CloseButton.OnButtonClick += CloseButton_OnButtonClick;
        }

        void CloseButton_OnButtonClick(UIElement button)
        {
            UIScreen.RemoveDialog(this);
        }
    }
}