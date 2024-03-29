﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FSO.SimAntics.Model.Routing
{
    public class VMWalkableRect : VMObstacle
    {
        public VMFreeList[] Free;
        public List<VMWalkableRect> Adj;

        //used directly for routing
        public VMWalkableRect Parent;
        public Point ParentSource;
        public int OriginalG;
        public int FScore;
        public int GScore;
        public bool Start;
        public byte State; //0 = untouched, 1 = open, 2 = closed;

        public VMWalkableRect(int x1, int y1, int x2, int y2) : base(x1,y1,x2,y2)
        {
            Free = new VMFreeList[4];
            Adj = new List<VMWalkableRect>();
        }
    }
}
