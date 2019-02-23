﻿using FSO.Client.Controllers;
using FSO.Common.Rendering.Framework.Camera;
using FSO.Common.Rendering.Framework.Model;
using FSO.LotView;
using Microsoft.Xna.Framework;

namespace FSO.Client.Rendering.City
{
    public interface ICityCamera : ICamera
    {
        TerrainZoomMode Zoomed { get; set; }
        float LotZoomProgress { get; set; }
        float ZoomProgress { get; set; }
        float LotSquish { get; }
        float FogMultiplier { get; }
        float DepthBiasScale { get; }
        bool HideUI { get; }

        void Update(UpdateState state, Terrain city);
        void MouseEvent(Common.Rendering.Framework.IO.UIMouseEventType type, UpdateState state);
        float GetIsoScale();
        Vector2 CalculateR();
        Vector2 CalculateRShadow();
        void InheritPosition(Terrain parent, World lotWorld, CoreGameScreenController controller);
    }
}
