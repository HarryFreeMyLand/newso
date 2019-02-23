/*
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
    /// Provides access to binding (*.bnd) data in FAR3 archives.
    /// </summary>
    public class AvatarBindingProvider : FAR3Provider<Binding>
    {
        public AvatarBindingProvider(GameContent contentManager)
            : base(contentManager, new BindingCodec(), new Regex(".*/bindings/.*\\.dat"))
        {
        }
    }
}
