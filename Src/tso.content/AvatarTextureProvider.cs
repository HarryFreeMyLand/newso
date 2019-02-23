/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using FSO.Content.Framework;
using FSO.Content.Codecs;
using System.Text.RegularExpressions;
using FSO.Content.Model;

namespace FSO.Content
{
    /// <summary>
    /// Provides access to texture (*.jpg) data in FAR3 archives.
    /// </summary>
    public class AvatarTextureProvider : FAR3Provider<ITextureRef> {
        public AvatarTextureProvider(GameContent contentManager)
            : base(contentManager, new TextureCodec(), new Regex(".*/textures/.*\\.dat"))
        {
        }
    }
}
