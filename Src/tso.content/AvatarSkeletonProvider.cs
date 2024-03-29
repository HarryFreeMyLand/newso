﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using FSO.Vitaboy;
using FSO.Content.Framework;
using System.Text.RegularExpressions;
using FSO.Content.Codecs;

namespace FSO.Content
{
    /// <summary>
    /// Provides access to skeleton (*.skel) data in FAR3 archives.
    /// </summary>
    public class AvatarSkeletonProvider : FAR3Provider<Skeleton>
    {
        public AvatarSkeletonProvider(GameContent contentManager)
            : base(contentManager, new SkeletonCodec(), new Regex(".*/skeletons/.*\\.dat"))
        {
        }
    }
}
