﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Files.Utils;
using Microsoft.Xna.Framework;

namespace FSO.Files.Formats.IFF.Chunks
{
    /// <summary>
    /// This format isn't documented on the wiki! Thanks, Darren!
    /// </summary>
    public class SLOT : IffChunk
    {

        public static float[] HeightOffsets = {
            //NOTE: 1 indexed! to get offset for a height, lookup (SLOT.Height-1)
            0, //floor
            2.5f, //low table
            4, //table
            4, //counter
            0, //non-standard (appears to use offset height)
            0, //in hand (unused probably. we handle avatar hands as a special case.)
            7, //sitting (used for chairs)
            4, //end table
            0 //TODO: unknown
        };

        public Dictionary<ushort, List<SLOTItem>> Slots;

        public override void Read(IffFile iff, System.IO.Stream stream)
        {
            using (var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN)){
                var zero = io.ReadUInt32();
                var version = io.ReadUInt32();
                var slotMagic = io.ReadBytes(4);
                var numSlots = io.ReadUInt32();

                Slots = new Dictionary<ushort, List<SLOTItem>>();

                /** The span for version 4 is 34. 
                 * The span for version 6 is 54. 
                 * The span for version 7 is 58.  
                 * The span for version 8 is 62. 
                 * The span for version 9 is 66. 
                 * The span for version 10 is 70.  **/
                for (var i = 0; i < numSlots; i++){
                    var item = new SLOTItem();
                    item.Type = io.ReadUInt16();
                    item.Offset = new Vector3(
                        io.ReadFloat(),
                        io.ReadFloat(),
                        io.ReadFloat()
                    );

                    var standing = io.ReadInt32();
                    var sitting = io.ReadInt32();
                    var ground = io.ReadInt32();
                    var rsflags = io.ReadInt32();
                    var snaptargetslot = io.ReadInt32();

                    //bonuses (0 means never)
                    item.Standing = standing; //score bonus for standing destinations
                    item.Sitting = sitting; //score bonus for sitting destinations
                    item.Ground = ground; //score bonus for sitting on ground 

                    item.Rsflags = (SLOTFlags)rsflags;
                    item.SnapTargetSlot = snaptargetslot;

                    if (version >= 6)
                    {
                        var minproximity = io.ReadInt32();
                        var maxproximity = io.ReadInt32();
                        var optimalproximity = io.ReadInt32();
                        var i9 = io.ReadInt32();
                        var i10 = io.ReadInt32();

                        item.MinProximity = minproximity;
                        item.MaxProximity = maxproximity;
                        item.OptimalProximity = optimalproximity;
                    }

                    if (version <= 9) {
                        item.MinProximity *= 16;
                        item.MaxProximity *= 16;
                        item.OptimalProximity *= 16;
                    }

                    if (version >= 7) item.Gradient = io.ReadFloat();

                    if (version >= 8) item.Height = io.ReadInt32();

                    //the below cases are just here for breakpoint purposes
                    //TODO: find use cases?
                    if (item.Height == 9) item.Height = 9;

                    if (item.Height == 0) item.Height = 5; //use offset height, nonstandard.

                    if (version >= 9) item.Facing = (SLOTFacing)io.ReadInt32();

                    if (version >= 10) item.Resolution = io.ReadInt32();

                    if (!Slots.ContainsKey(item.Type)) Slots.Add(item.Type, new List<SLOTItem>());
                    Slots[item.Type].Add(item);
                }
            }
        }
    }

    [Flags]
    public enum SLOTFlags : int
    {
        NORTH = 1,
        NORTH_EAST = 2,
        EAST = 4,
        SOUTH_EAST = 8,
        SOUTH = 16,
        SOUTH_WEST = 32,
        WEST = 64,
        NORTH_WEST = 128,
        AllowAnyRotation = 256,
        Absolute = 512,
        FacingAwayFromObject = 1024,
        IgnoreRooms = 2048,
        SnapToDirection = 4096,
        RandomScoring=8192,
        AllowFailureTrees = 16385,
        AllowDifferentAlts=32768,
        UseAverageObjectLocation = 65536
    }

    public enum SLOTFacing : int
    {
        FaceAnywhere = -3,
        FaceTowardsObject = -2,
        FaceAwayFromObject = -1,
    }

    public class SLOTItem
    {
        public ushort Type;
        public Vector3 Offset;
        public int Standing = 1;
        public int Sitting = 0;
        public int Ground = 0;
        public SLOTFlags Rsflags;
        public int SnapTargetSlot = -1;
        public int MinProximity;
        public int MaxProximity = 0;
        public int OptimalProximity = 0;
        public float Gradient;
        public SLOTFacing Facing = SLOTFacing.FaceTowardsObject;
        public int Resolution = 16;
        public int Height;
    }
}
