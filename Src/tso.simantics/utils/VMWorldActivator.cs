﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.LotView;
using FSO.LotView.Model;
using FSO.LotView.Components;
using FSO.Content;
using Microsoft.Xna.Framework;
using FSO.Files.Formats.IFF.Chunks;
using FSO.SimAntics.Model;

namespace FSO.SimAntics.Utils
{
    /// <summary>
    /// Handles object creation and destruction
    /// </summary>
    public class VMWorldActivator
    {
        private VM VM;
        private LotView.World World;
        private Blueprint Blueprint;

        public VMWorldActivator(VM vm, LotView.World world){
            this.VM = vm;
            this.World = world;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        public Blueprint LoadFromXML(XmlHouseData model){
            this.Blueprint = new Blueprint(model.Size, model.Size);
            VM.Context.Blueprint = Blueprint;
            VM.Context.Architecture = new VMArchitecture(model.Size, model.Size, Blueprint, VM.Context);

            var arch = VM.Context.Architecture;

            foreach (var floor in model.World.Floors){
                arch.SetFloor(floor.X, floor.Y, (sbyte)(floor.Level+1), new FloorTile { Pattern = (ushort)floor.Value }, true);
            }

            foreach (var pool in model.World.Pools)
            {
                arch.SetFloor(pool.X, pool.Y, 1, new FloorTile { Pattern = 65535 }, true);
            }

            foreach (var wall in model.World.Walls)
            {
                arch.SetWall((short)wall.X, (short)wall.Y, (sbyte)(wall.Level+1), new WallTile() //todo: these should read out in their intended formats - a cast shouldn't be necessary
                {
                    Segments = wall.Segments,
                    TopLeftPattern = (ushort)wall.TopLeftPattern,
                    TopRightPattern = (ushort)wall.TopRightPattern,
                    BottomLeftPattern = (ushort)wall.BottomLeftPattern,
                    BottomRightPattern = (ushort)wall.BottomRightPattern,
                    TopLeftStyle = (ushort)wall.LeftStyle,
                    TopRightStyle = (ushort)wall.RightStyle
                });
            }
            arch.RegenRoomMap();
            VM.Context.RegeneratePortalInfo();

            foreach (var obj in model.Objects)
            {
                //if (obj.Level == 0) continue;
                //if (obj.GUID == "0xE9CEB12F") obj.GUID = "0x01A0FD79"; //replace onlinejobs door with a normal one
                //if (obj.GUID == "0x346FE2BC") obj.GUID = "0x98E0F8BD"; //replace kitchen door with a normal one
                CreateObject(obj);
            }

            if (VM.UseWorld)
            {
                foreach (var obj in model.Sounds)
                {
                    VM.Context.Ambience.SetAmbience(VM.Context.Ambience.GetAmbienceFromGUID(obj.ID), (obj.On == 1));
                    World.State.WorldSize = model.Size;
                    
                }
                Blueprint.Terrain = CreateTerrain(model);
            }

            var testObject = new XmlHouseDataObject(); //test npc controller, not normally present on a job lot.
            testObject.GUID = "0x70F69082";
            testObject.X = 0;
            testObject.Y = 0;
            testObject.Level = 1;
            testObject.Dir = 0;
            CreateObject(testObject);

            arch.Tick();
            return this.Blueprint;
        }

        private TerrainComponent CreateTerrain(XmlHouseData model)
        {
            var terrain = new TerrainComponent(new Rectangle(1, 1, model.Size - 2, model.Size - 2));
            this.InitWorldComponent(terrain);
            return terrain;
        }

        public VMAvatar CreateAvatar()
        {
            return (VMAvatar)VM.Context.CreateObjectInstance(VMAvatar.TEMPLATE_PERSON, LotTilePos.OUT_OF_WORLD, Direction.NORTH).Objects[0];
        }

        public VMEntity CreateObject(XmlHouseDataObject obj){
            LotTilePos pos = (obj.Level == 0) ? LotTilePos.OUT_OF_WORLD : LotTilePos.FromBigTile((short)obj.X, (short)obj.Y, (sbyte)obj.Level);
            var nobj = VM.Context.CreateObjectInstance(obj.GUIDInt, pos, obj.Direction).Objects[0];

            if (obj.Group != 0)
            {
                foreach (var sub in nobj.MultitileGroup.Objects)
                {
                    sub.SetValue(VMStackObjectVariable.GroupID, (short)obj.Group);
                }
            }

            for (int i = 0; i < nobj.MultitileGroup.Objects.Count; i++) nobj.MultitileGroup.Objects[i].ExecuteEntryPoint(11, VM.Context, true);

            return nobj;
            
        }


        private void InitWorldComponent(WorldComponent component)
        {
            component.Initialize(this.World.State.Device, this.World.State);
        }

    }
}
