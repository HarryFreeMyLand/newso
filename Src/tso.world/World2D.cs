﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using FSO.LotView.Model;
using Microsoft.Xna.Framework;
using FSO.Common.Utils;
using FSO.Common.Rendering.Framework;
using FSO.LotView.Components;
using System.IO;
using FSO.LotView.Utils;

namespace FSO.LotView
{
    /// <summary>
    /// Handles rendering the 2D world
    /// </summary>
    public class World2D
    {
        public static SurfaceFormat[] BUFFER_SURFACE_FORMATS = new SurfaceFormat[] {
            /** Thumbnail buffer **/
            SurfaceFormat.Color,

            /** Static Object Buffers **/
            SurfaceFormat.Color,
            /** Depth buffer must be single surface format for precision reasons **/
            SurfaceFormat.Single,

            /** Terrain Color **/
            SurfaceFormat.Color,

            /** Object ID buffer **/
            SurfaceFormat.Single,

            /** Archetecture buffers **/
            SurfaceFormat.Color,
            SurfaceFormat.Single,

            /** Terrain Depth **/
            SurfaceFormat.Color
        };

        public static readonly int NUM_2D_BUFFERS = 8;
        public static readonly int BUFFER_THUMB = 0; //used for drawing thumbnails
        public static readonly int BUFFER_STATIC_OBJECTS_PIXEL = 1;
        public static readonly int BUFFER_STATIC_OBJECTS_DEPTH = 2;
        public static readonly int BUFFER_STATIC_TERRAIN = 3;
        public static readonly int BUFFER_OBJID = 4;
        public static readonly int BUFFER_ARCHETECTURE_PIXEL = 5;
        public static readonly int BUFFER_ARCHETECTURE_DEPTH = 6;
        public static readonly int BUFFER_STATIC_TERRAIN_DEPTH = 7;


        public static readonly int SCROLL_BUFFER = 512; //resolution to add to render size for scroll reasons


        private Blueprint Blueprint;
        private Dictionary<WorldComponent, WorldObjectRenderInfo> RenderInfo = new Dictionary<WorldComponent, WorldObjectRenderInfo>();

        private List<_2DDrawGroup> StaticObjectsCache = new List<_2DDrawGroup>();
        private ScrollBuffer StaticObjects;

        private List<_2DDrawGroup> StaticArchCache = new List<_2DDrawGroup>();
        private ScrollBuffer StaticArch;

        private int TicksSinceLight = 0;

        public void Init(Blueprint blueprint)
        {
            this.Blueprint = blueprint;
        }

        private WorldObjectRenderInfo GetRenderInfo(WorldComponent component)
        {
            return ((ObjectComponent)component).renderInfo;
        }

        /// <summary>
        /// Gets an object's ID given an object's screen position.
        /// </summary>
        /// <param name="x">The object's X position.</param>
        /// <param name="y">The object's Y position.</param>
        /// <param name="gd">GraphicsDevice instance.</param>
        /// <param name="state">WorldState instance.</param>
        /// <returns>Object's ID if the object was found at the given position.</returns>
        public short GetObjectIDAtScreenPos(int x, int y, GraphicsDevice gd, WorldState state)
        {
            /** Draw all objects to a texture as their IDs **/
            var oldCenter = state.CenterTile;
            var tileOff = state.WorldSpace.GetTileFromScreen(new Vector2(x, y));
            state.CenterTile += tileOff;
            var pxOffset = state.WorldSpace.GetScreenOffset();
            var _2d = state._2D;
            Promise<Texture2D> bufferTexture = null;

            var worldBounds = new Rectangle((-pxOffset).ToPoint(), new Point(1, 1));

            state.TempDraw = true;
            state._2D.OBJIDMode = true;
            state._3D.OBJIDMode = true;
            using (var buffer = state._2D.WithBuffer(BUFFER_OBJID, ref bufferTexture))
            {
                _2d.SetScroll(-pxOffset);
                
                while (buffer.NextPass())
                {
                    foreach (var obj in Blueprint.Objects) { 

                                var tilePosition = obj.Position;

                                if (obj.Level != state.Level) continue;

                                var oPx = state.WorldSpace.GetScreenFromTile(tilePosition);
                                obj.ValidateSprite(state);
                                var offBound = new Rectangle(obj.Bounding.Location + oPx.ToPoint(), obj.Bounding.Size);
                                if (!offBound.Intersects(worldBounds)) continue;

                                var renderInfo = GetRenderInfo(obj);
                                
                                _2d.OffsetPixel(oPx);
                                _2d.OffsetTile(tilePosition);
                                _2d.SetObjID(obj.ObjectID);
                                obj.Draw(gd, state);
                    }

                    state._3D.Begin(gd);
                    foreach (var avatar in Blueprint.Avatars)
                    {
                        _2d.OffsetPixel(state.WorldSpace.GetScreenFromTile(avatar.Position));
                        _2d.OffsetTile(avatar.Position);
                        avatar.Draw(gd, state);
                    }
                    state._3D.End();
                }
                
            }
            state._3D.OBJIDMode = false;
            state._2D.OBJIDMode = false;
            state.TempDraw = false;
            state.CenterTile = oldCenter;

            var tex = bufferTexture.Get();
            Single[] data = new float[1];
            tex.GetData<Single>(data);
            return (short)Math.Round(data[0]*65535f);
        }

        /// <summary>
        /// Gets an object group's thumbnail provided an array of objects.
        /// </summary>
        /// <param name="objects">The object components to draw.</param>
        /// <param name="gd">GraphicsDevice instance.</param>
        /// <param name="state">WorldState instance.</param>
        /// <returns>Object's ID if the object was found at the given position.</returns>
        public Texture2D GetObjectThumb(ObjectComponent[] objects, Vector3[] positions, GraphicsDevice gd, WorldState state)
        {
            var oldZoom = state.Zoom;
            var oldRotation = state.Rotation;
            /** Center average position **/
            Vector3 average = new Vector3();
            for (int i = 0; i < positions.Length; i++)
            {
                average += positions[i];
            }
            average /= positions.Length;

            state.SilentZoom = WorldZoom.Near;
            state.SilentRotation = WorldRotation.BottomRight;
            state.WorldSpace.Invalidate();
            state.InvalidateCamera();
            state.TempDraw = true;
            var pxOffset = new Vector2(442, 275) - state.WorldSpace.GetScreenFromTile(average);

            var _2d = state._2D;
            Promise<Texture2D> bufferTexture = null;
            state._2D.OBJIDMode = false;
            Rectangle bounds = new Rectangle();
            using (var buffer = state._2D.WithBuffer(BUFFER_THUMB, ref bufferTexture))
            {
                _2d.SetScroll(new Vector2());
                while (buffer.NextPass())
                {
                    for (int i=0; i<objects.Length; i++)
                    {
                        var obj = objects[i];
                        var tilePosition = positions[i];

                        //we need to trick the object into believing it is in a set world state.
                        var oldObjRot = obj.Direction;
                        var oldRoom = obj.Room;

                        obj.Direction = Direction.NORTH;
                        obj.Room = 65535;
                        state.SilentZoom = WorldZoom.Near;
                        state.SilentRotation = WorldRotation.BottomRight;
                        obj.OnRotationChanged(state);
                        obj.OnZoomChanged(state);

                        _2d.OffsetPixel(state.WorldSpace.GetScreenFromTile(tilePosition) + pxOffset);
                        _2d.OffsetTile(tilePosition);
                        _2d.SetObjID(obj.ObjectID);
                        obj.Draw(gd, state);

                        //return everything to normal
                        obj.Direction = oldObjRot;
                        obj.Room = oldRoom;
                        state.SilentZoom = oldZoom;
                        state.SilentRotation = oldRotation;
                        obj.OnRotationChanged(state);
                        obj.OnZoomChanged(state);
                    }
                    bounds = _2d.GetSpriteListBounds();
                }
            }
            bounds.X = Math.Max(0, Math.Min(1023, bounds.X));
            bounds.Y = Math.Max(0, Math.Min(1023, bounds.Y));
            if (bounds.Width + bounds.X > 1024) bounds.Width = 1024 - bounds.X;
            if (bounds.Height + bounds.Y > 1024) bounds.Height = 1024 - bounds.Y;

            //return things to normal
            state.WorldSpace.Invalidate();
            state.InvalidateCamera();
            state.TempDraw = false;

            var tex = bufferTexture.Get();
            return TextureUtils.Clip(gd, tex, bounds);
        }

        /// <summary>
        /// Prep work before screen is painted
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="state"></param>
        public void PreDraw(GraphicsDevice gd, WorldState state)
        {
            var pxOffset = -state.WorldSpace.GetScreenOffset();
            var damage = Blueprint.Damage;
            var _2d = state._2D;

            /**
             * Tasks:
             *  If zoom or rotation has changed, redraw all static layers
             *  If scroll has changed, redraw static layer if the scroll is outwith the buffered region
             *  If architecture has changed, redraw appropriate static layer
             *  If there is a new object in the static layer, redraw the static layer
             *  If an objects in the static layer has changed, redraw the static layer and move the object to the dynamic layer
             *  If wall visibility has changed, redraw wall layer (should think about how this works with breakthrough wall mode
             */

            var redrawStaticObjects = false;
            var redrawWalls = false;

            var recacheWalls = false;
            var recacheObjects = false;

            if (TicksSinceLight++ > 60 * 4) damage.Add(new BlueprintDamage(BlueprintDamageType.LIGHTING_CHANGED));

            WorldObjectRenderInfo info = null;

            foreach (var item in damage){
                switch (item.Type){
                    case BlueprintDamageType.ROTATE:
                    case BlueprintDamageType.ZOOM:
                    case BlueprintDamageType.LEVEL_CHANGED:
                        recacheObjects = true;
                        recacheWalls = true;
                        redrawWalls = true;
                        redrawStaticObjects = true;
                        break;
                    case BlueprintDamageType.SCROLL:
                        if (StaticObjects == null || StaticObjects.PxOffset != GetScrollIncrement(pxOffset))
                        {
                            redrawWalls = true;
                            redrawStaticObjects = true;
                        }
                        break;
                    case BlueprintDamageType.LIGHTING_CHANGED:
                        redrawWalls = true;
                        redrawStaticObjects = true;

                        Blueprint.GenerateRoomLights();
                        state.OutsideColor = Blueprint.RoomColors[1];
                        state._3D.RoomLights = Blueprint.RoomColors;
                        state._2D.AmbientLight.SetData(Blueprint.RoomColors);
                        TicksSinceLight = 0;
                        break;
                    case BlueprintDamageType.OBJECT_MOVE:
                        /** Redraw if its in static layer **/
                        info = GetRenderInfo(item.Component);
                        if (info.Layer == WorldObjectRenderLayer.STATIC){
                            redrawStaticObjects = true;
                            recacheObjects = true;
                            info.Layer = WorldObjectRenderLayer.DYNAMIC;
                        }
                        if (item.Component is ObjectComponent) ((ObjectComponent)item.Component).DynamicCounter = 0;
                        break;
                    case BlueprintDamageType.OBJECT_GRAPHIC_CHANGE:
                        /** Redraw if its in static layer **/
                        info = GetRenderInfo(item.Component);
                        if (info.Layer == WorldObjectRenderLayer.STATIC){
                            redrawStaticObjects = true;
                            recacheObjects = true;
                            info.Layer = WorldObjectRenderLayer.DYNAMIC;
                        }
                        if (item.Component is ObjectComponent) ((ObjectComponent)item.Component).DynamicCounter = 0;
                        break;
                    case BlueprintDamageType.OBJECT_RETURN_TO_STATIC:
                        info = GetRenderInfo(item.Component);
                        if (info.Layer == WorldObjectRenderLayer.DYNAMIC)
                        {
                            redrawStaticObjects = true;
                            recacheObjects = true;
                            info.Layer = WorldObjectRenderLayer.STATIC;
                        }
                        break;
                    case BlueprintDamageType.WALL_CUT_CHANGED:
                    case BlueprintDamageType.FLOOR_CHANGED:
                    case BlueprintDamageType.WALL_CHANGED:
                        redrawWalls = true;
                        recacheWalls = true;
                        break;
                }
            }
            damage.Clear();

            var tileOffset = state.WorldSpace.GetTileFromScreen(-pxOffset);
            //scroll buffer loads in increments of SCROLL_BUFFER
            var newOff = GetScrollIncrement(pxOffset);
            var oldCenter = state.CenterTile;
            state.CenterTile += state.WorldSpace.GetTileFromScreen(newOff-pxOffset); //offset the scroll to the position of the scroll buffer.
            tileOffset = state.CenterTile;

            pxOffset = newOff;

            if (recacheWalls)
            {
                _2d.Pause();
                _2d.Resume(); //clear the sprite buffer before we begin drawing what we're going to cache
                Blueprint.Terrain.RegenTerrain(gd, state, Blueprint);
                Blueprint.FloorComp.Draw(gd, state);
                Blueprint.WallComp.Draw(gd, state);
                StaticArchCache.Clear();
                _2d.End(StaticArchCache, true);
            }

            if (redrawWalls)
            {
                /** Draw archetecture to a texture **/
                Promise<Texture2D> bufferTexture = null;
                Promise<Texture2D> depthTexture = null;
                using (var buffer = state._2D.WithBuffer(BUFFER_ARCHETECTURE_PIXEL, ref bufferTexture, BUFFER_ARCHETECTURE_DEPTH, ref depthTexture))
                {
                    _2d.SetScroll(pxOffset);
                    while (buffer.NextPass())
                    {
                        _2d.RenderCache(StaticArchCache);
                        Blueprint.Terrain.Draw(gd, state);
                    }
                }
                StaticArch = new ScrollBuffer(bufferTexture.Get(), depthTexture.Get(), pxOffset, new Vector3(tileOffset, 0));
            }

            if (recacheObjects)
            {
                _2d.Pause();
                _2d.Resume();

                foreach (var obj in Blueprint.Objects)
                {
                    if (obj.Level > state.Level) continue;
                    var renderInfo = GetRenderInfo(obj);
                    if (renderInfo.Layer == WorldObjectRenderLayer.STATIC)
                    {
                        var tilePosition = obj.Position;
                        _2d.OffsetPixel(state.WorldSpace.GetScreenFromTile(tilePosition));
                        _2d.OffsetTile(tilePosition);
                        _2d.SetObjID(obj.ObjectID);
                        obj.Draw(gd, state);
                    }

                }
                StaticObjectsCache.Clear();
                _2d.End(StaticObjectsCache, true);
            }

            if (redrawStaticObjects){
                /** Draw static objects to a texture **/
                Promise<Texture2D> bufferTexture = null;
                Promise<Texture2D> depthTexture = null;
                using (var buffer = state._2D.WithBuffer(BUFFER_STATIC_OBJECTS_PIXEL, ref bufferTexture, BUFFER_STATIC_OBJECTS_DEPTH, ref depthTexture))
                {
                    _2d.SetScroll(pxOffset);
                    while (buffer.NextPass())
                    {
                        _2d.RenderCache(StaticObjectsCache);
                    }
                }

                StaticObjects = new ScrollBuffer(bufferTexture.Get(), depthTexture.Get(), pxOffset, new Vector3(tileOffset, 0));
            }
            state.CenterTile = oldCenter; //revert to our real scroll position
        }

        public Vector2 GetScrollIncrement(Vector2 pxOffset)
        {
            return new Vector2((float)Math.Floor(pxOffset.X / SCROLL_BUFFER) * SCROLL_BUFFER, (float)Math.Floor(pxOffset.Y / SCROLL_BUFFER) * SCROLL_BUFFER);
        }

        /// <summary>
        /// Paint to screen
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="state"></param>
        public void Draw(GraphicsDevice gd, WorldState state){

            var _2d = state._2D;
            /**
             * Draw static layers
             */
            _2d.OffsetPixel(Vector2.Zero);
            _2d.SetScroll(new Vector2());

            var pxOffset = -state.WorldSpace.GetScreenOffset();
            var tileOffset = state.CenterTile;
            if (StaticArch != null)
                _2d.DrawScrollBuffer(StaticArch, pxOffset, new Vector3(tileOffset, 0), state);
            if (StaticObjects != null)
                _2d.DrawScrollBuffer(StaticObjects, pxOffset, new Vector3(tileOffset, 0), state);

            _2d.End();
            _2d.Begin(state.Camera);

            /**
             * Draw dynamic objects. If an object has been static for X frames move it back into the static layer
             */

            _2d.SetScroll(pxOffset);

            var size = new Vector2(state.WorldSpace.WorldPxWidth, state.WorldSpace.WorldPxHeight);
            var mainBd = state.WorldSpace.GetScreenFromTile(state.CenterTile);
            var diff = pxOffset - mainBd;
            var worldBounds = new Rectangle((pxOffset).ToPoint(), size.ToPoint());

            foreach (var obj in Blueprint.Objects)
            {
                if (obj.Level > state.Level) continue;
                var renderInfo = GetRenderInfo(obj);
                if (renderInfo.Layer == WorldObjectRenderLayer.DYNAMIC)
                {
                    var tilePosition = obj.Position;
                    var oPx = state.WorldSpace.GetScreenFromTile(tilePosition);
                    obj.ValidateSprite(state);
                    var offBound = new Rectangle(obj.Bounding.Location + oPx.ToPoint(), obj.Bounding.Size);
                    if (!offBound.Intersects(worldBounds)) continue;
                    _2d.OffsetPixel(oPx);
                    _2d.OffsetTile(tilePosition);
                    _2d.SetObjID(obj.ObjectID);
                    obj.Draw(gd, state);
                }
            }
        }
    }

    public class WorldObjectRenderInfo
    {
        public WorldObjectRenderLayer Layer = WorldObjectRenderLayer.STATIC;
    }

    public enum WorldObjectRenderLayer
    {
        STATIC,
        DYNAMIC
    }

    public struct WorldTileRenderingInfo
    {
        public bool Dirty;
        public Texture2D Pixel;
        public Texture2D ZBuffer;
    }
}
