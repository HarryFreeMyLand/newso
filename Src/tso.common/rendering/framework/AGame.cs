/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FSO.Common.Rendering.Framework
{
    // Abstract Game
    public abstract class AGame : Game
    {
        protected GraphicsDeviceManager Graphics;
        protected GameScreen Screen;

        public AGame() : base()
        {
            Graphics = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            base.Initialize();

            Screen = new GameScreen(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            Screen.Update(gameTime, IsActive);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Screen.Draw(gameTime);
        }
    }
}
