﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using FSO.Content.Framework;
using FSO.Content.Codecs;
using FSO.Vitaboy;
using System.Text.RegularExpressions;

namespace FSO.Content
{
    /// <summary>
    /// Provides access to handgroup (*.hag) data in FAR3 archives.
    /// </summary>
    public class HandgroupProvider : FAR3Provider<HandGroup>
    {
        public HandgroupProvider(GameContent contentManager)
            : base(contentManager, new HandgroupCodec(), new Regex(".*/hands/groups/.*\\.dat"))
        {
        }
    }
}