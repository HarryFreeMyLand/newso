﻿/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using FSO.Client.UI.Framework;
using Microsoft.Xna.Framework.Input;
using FSO.Client.UI.Model;
using FSO.Common.Rendering.Framework.IO;
using FSO.Common.Rendering.Framework.Model;
using Microsoft.Xna.Framework;

namespace FSO.Client.UI.Controls
{
    public class UIRectangle : UIElement
    {
        private Color color = Color.White;

        public UIRectangle()
        {
            ListenForMouse(new Microsoft.Xna.Framework.Rectangle(0, 0, 50, 50), new UIMouseEvent(OnMouse));
        }

        private bool isDown = false;

        private void OnMouse(UIMouseEventType type, UpdateState state)
        {
            if (type == UIMouseEventType.MouseOver)
            {
                if (isDown) { return; }
                color = Color.Red;
            }
            else if (type == UIMouseEventType.MouseOut)
            {
                if (isDown) { return; }
                color = Color.White;
            }
            else if (type == UIMouseEventType.MouseDown)
            {
                color = Color.Blue;
                isDown = true;
            }
            else if (type == UIMouseEventType.MouseUp)
            {
                isDown = false;
                color = Color.Green;
            }
        }


        //public override void Update(TSOClient.Code.UI.Model.UpdateState state)
        //{
        //    base.Update(state);

        //    /** Hit test **/
        //    color = Color.White;
        //    if (HitTestArea(state, new Microsoft.Xna.Framework.Rectangle(0, 0, 50, 50)))
        //    {
                
        //    }

        //    //color
        //}


        public override void Draw(UISpriteBatch batch)
        {
            var whiteRectangle = new Texture2D(batch.GraphicsDevice, 1, 1);
            whiteRectangle.SetData(new[] { color });

            var pos = LocalRect(0, 0, 50, 50);
            batch.Draw(whiteRectangle, pos, Color.White);
        }
    }
}
