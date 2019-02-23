﻿using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common.Enum;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FSO.Client.UI.Panels
{
    public class UILotSkillModeDialog : UIDialog
    {
        public static Dictionary<LotCategory, uint> SkillGameplayCategory = new Dictionary<LotCategory, uint>()
        {
            { LotCategory.entertainment, 1 },
            { LotCategory.services, 1 },
            { LotCategory.romance, 1 },
            { LotCategory.welcome, 1 }
        };

        public UILabel DescLabel;
        public event Action<uint> OnModeChosen;
        public uint Result;

        public UILotSkillModeDialog(LotCategory category, uint originalValue) : base(UIDialogStyle.OK | UIDialogStyle.Close, true)
        {
            SetSize(400, 300);

            uint min = 0;
            if (!SkillGameplayCategory.TryGetValue(category, out min)) min = 0;

            Caption = GameFacade.Strings.GetString("f109", "5");
            DescLabel = new UILabel
            {
                Caption = GameFacade.Strings.GetString("f109", "6") + ((min > 0) ? ("\n\n" + GameFacade.Strings.GetString("f109", "7")) : ""),
                Position = new Vector2(25, 40),
                Wrapped = true,
                Size = new Vector2(350, 200)
            };
            Add(DescLabel);

            var vbox = new UIVBoxContainer();
            for (uint i=0; i<3; i++)
            {
                var hbox = new UIHBoxContainer();
                var radio = new UIRadioButton
                {
                    RadioGroup = "skl",
                    RadioData = i,
                    Disabled = i < min,
                    Selected = i == originalValue
                };
                radio.OnButtonClick += Radio_OnButtonClick;

                hbox.Add(radio);
                hbox.Add(new UILabel
                {
                    Caption = GameFacade.Strings.GetString("f109", (8 + i).ToString())
                });
                vbox.Add(hbox);
            }
            vbox.Position = new Vector2(25, 200);
            Add(vbox);
            vbox.AutoSize();

            CloseButton.OnButtonClick += CloseButton_OnButtonClick;
            OKButton.OnButtonClick += OKButton_OnButtonClick;
        }

        void OKButton_OnButtonClick(UIElement button)
        {
            UIScreen.RemoveDialog(this);
            OnModeChosen(Result);
        }

        void CloseButton_OnButtonClick(UIElement button)
        {
            UIScreen.RemoveDialog(this);
        }

        void Radio_OnButtonClick(UIElement button)
        {
            Result = (uint)((UIRadioButton)button).RadioData;
        }
    }
}