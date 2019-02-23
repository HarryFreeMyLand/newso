/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using FSO.Client.UI.Framework;
using Microsoft.Xna.Framework.Graphics;
using FSO.Client.UI.Controls;

namespace FSO.Client.UI.Screens
{
    public class Credits : GameScreen
    {
        public Texture2D BackgroundImage { get; set; }
        public UIButton BackButton { get; set; }
        public UIButton OkButton { get; set; }

        public Credits()
        {
            var ui = RenderScript("credits.uis");

            X = (float)(double)(ScreenWidth - 800) / 2;
            Y = (float)(double)(ScreenHeight - 600) / 2;

            AddAt(0, new UIImage(BackgroundImage));
            Add(ui.Create<UIImage>("TSOLogoImage"));


            BackButton.OnButtonClick += new ButtonClickDelegate(BackButton_OnButtonClick);
            OkButton.OnButtonClick += new ButtonClickDelegate(BackButton_OnButtonClick);
        }

        void BackButton_OnButtonClick(UIElement button)
        {
            GameFacade.Screens.RemoveScreen(this);
        }
    }
}
