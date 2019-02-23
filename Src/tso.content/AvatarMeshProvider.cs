/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using FSO.Content.Framework;
using Microsoft.Xna.Framework.Graphics;
using FSO.Content.Codecs;
using System.Text.RegularExpressions;
using FSO.Vitaboy;

namespace FSO.Content
{
    /// <summary>
    /// Provides access to mesh (*.mesh) data in FAR3 archives.
    /// </summary>
    public class AvatarMeshProvider : FAR3Provider<Mesh>{
        public AvatarMeshProvider(GameContent contentManager, GraphicsDevice device) : base(contentManager, new MeshCodec(), new Regex(".*/meshes/.*\\.dat"))
        {
        }
    }
}
