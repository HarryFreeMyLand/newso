/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System.Collections.Generic;
using FSO.Content.Framework;
using FSO.Content.Codecs;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
using FSO.Content.Model;
using System.IO;

namespace FSO.Content
{
    /// <summary>
    /// Provides access to UI texture (*.bmp) data in FAR3 archives.
    /// </summary>
    public class UIGraphicsProvider : FAR3Provider<ITextureRef>
    {
        public static uint[] MASK_COLORS = new uint[]{
            new Color(0xFF, 0x00, 0xFF, 0xFF).PackedValue,
            new Color(0xFE, 0x02, 0xFE, 0xFF).PackedValue,
            new Color(0xFF, 0x01, 0xFF, 0xFF).PackedValue
        };

        private Dictionary<ulong, string> _files = new Dictionary<ulong, string>();
        private Dictionary<ulong, ITextureRef> _filesCache = new Dictionary<ulong, ITextureRef>();

        //For some reason, the rack eod has a graphic id that we don't, but the file does exist under another iD.
        //Can't see any problem with file parser so putting in a mapping for now
        private Dictionary<ulong, ulong> _pointers = new Dictionary<ulong, ulong>();


        public UIGraphicsProvider(GameContent contentManager)
            : base(contentManager, new TextureCodec(MASK_COLORS), new Regex("uigraphics/.*\\.dat"))
        {
            _files[0x00000Cb800000002] = "uigraphics/friendshipweb/friendshipwebalpha.tga";
            _files[0x00000Cbfb00000001] = "uigraphics/hints/hint_mechanicskill.bmp";

            _files[0x1AF0856DDBAC] = "uigraphics/chat/balloonpointersadbottom.bmp";
            _files[0x1B00856DDBAC] = "uigraphics/chat/balloonpointersadside.bmp";
            _files[0x1B10856DDBAC] = "uigraphics/chat/balloontilessad.bmp";

            _files[0x1972454856DDBAC] = "uigraphics/friendshipweb/f_web_inbtn.bmp";
            _files[0x3D3AEF0856DDBAC] = "uigraphics/friendshipweb/f_web_outbtn.bmp";
            //./uigraphics/eods/costumetrunk/eod_costumetrunkbodySkinBtn.bmp
            _pointers[0x0000028800000001] = 0x0000094600000001;
        }

        protected override ITextureRef ResolveById(ulong id)
        {
            if (_pointers.ContainsKey(id))
            {
                id = _pointers[id];
            }
            if (_files.ContainsKey(id))
            {
                //Non far3 file
                if (_filesCache.ContainsKey(id))
                { return _filesCache[id]; }
                var path = this.ContentManager.GetPath(_files[id]);
                using (var stream = File.OpenRead(path))
                {
                    _filesCache.Add(id, Codec.Decode(stream));
                    return _filesCache[id];
                }
            }
            return base.ResolveById(id);
        }
    }
}