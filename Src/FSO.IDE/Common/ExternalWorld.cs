﻿using FSO.Common;
using FSO.Common.Rendering.Framework;
using FSO.Common.Utils;
using FSO.LotView;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.IDE.Common
{
    public class ExternalWorld : World
    {
        public ExternalWorld(GraphicsDevice Device)
            : base(Device)
        {
        }

        public override void Initialize(_3DLayer layer)
        {
            /**
             * Setup world state, this object acts as a facade
             * to world objects as well as providing various
             * state settings for the world and helper functions
             */
            State = new WorldState(layer.Device, layer.Device.Viewport.Width / FSOEnvironment.DPIScaleFactor, layer.Device.Viewport.Height / FSOEnvironment.DPIScaleFactor, this)
            {
                AmbientLight = new Texture2D(layer.Device, 256, 256),
                OutsidePx = new Texture2D(layer.Device, 1, 1)
            };
            State._3D = new LotView.Utils._3DWorldBatch(State);
            State._2D = new LotView.Utils._2DWorldBatch(layer.Device, 2, new SurfaceFormat[] {
                World2D.BUFFER_SURFACE_FORMATS[0],
                World2D.BUFFER_SURFACE_FORMATS[World2D.BUFFER_THUMB_DEPTH] }, new bool[] { true, false }, World2D.SCROLL_BUFFER)
            {
                AdvLight = TextureGenerator.GetPxWhite(layer.Device)
            };
            State.DrawOOB = true;
            UseBackbuffer = false;

            Camera = State.Camera;

            HasInitGPU = true;
            HasInit = HasInitGPU & HasInitBlueprint;
        }
    }
}