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
using Microsoft.Xna.Framework;

namespace FSO.Common.Utils
{
    public class TextureUtils
    {
        public static Texture2D TextureFromColor(GraphicsDevice gd, Color color)
        {
            var tex = new Texture2D(gd, 1, 1);
            tex.SetData(new[] { color });
            return tex;
        }

        public static Texture2D TextureFromColor(GraphicsDevice gd, Color color, int width, int height)
        {
            var tex = new Texture2D(gd, width, height);
            var data = new Color[width * height];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }
            tex.SetData(data);
            return tex;
        }

        /**
         * Because the buffers can be fairly big, its much quicker to just keep some
         * in memory and reuse them for resampling textures
         * 
         * rhy: yeah, maybe, if the code actually did that. i'm also not sure about keeping ~32MB 
         * of texture buffers in memory at all times when the game is largely single threaded.
         */
        private static List<uint[]> ResampleBuffers = new List<uint[]>();
        private static ulong MaxResampleBufferSize = 1024 * 768;

        static TextureUtils()
        {
            /*for (var i = 0; i < 10; i++)
            {
                ResampleBuffers.Add(new uint[MaxResampleBufferSize]);
            }*/
        }

        private static uint[] GetBuffer(int size) //todo: maybe implement something like described, old implementation was broken
        {
            var newBuffer = new uint[size];
            return newBuffer;
        }

        private static void FreeBuffer(uint[] buffer)
        {
        }

        public static void MaskFromTexture(ref Texture2D Texture, Texture2D Mask, uint[] ColorsFrom)
        {
            if (Texture.Width != Mask.Width || Texture.Height != Mask.Height)
            {
                return;
            }

            var ColorTo = Color.Transparent.PackedValue;

            var size = Texture.Width * Texture.Height;
            uint[] buffer = GetBuffer(size);
            Texture.GetData(buffer, 0, size);

            var sizeMask = Mask.Width * Mask.Height;
            var bufferMask = GetBuffer(sizeMask);
            Mask.GetData(bufferMask, 0, sizeMask);

            var didChange = false;
            for (int i = 0; i < size; i++)
            {
                if (ColorsFrom.Contains(bufferMask[i]))
                {
                    didChange = true;
                    buffer[i] = ColorTo;
                }
            }

            if (didChange)
            {
                Texture.SetData(buffer, 0, size);
            }
        }

        public static Texture2D Clip(GraphicsDevice gd, Texture2D texture, Rectangle source)
        {
            var newTexture = new Texture2D(gd, source.Width, source.Height);

            var size = source.Width * source.Height;
            uint[] buffer = GetBuffer(size);
            texture.GetData(0, source, buffer, 0, size);

            newTexture.SetData(buffer, 0, size);
            return newTexture;
        }

        public static Texture2D Copy(GraphicsDevice gd, Texture2D texture)
        {
            var newTexture = new Texture2D(gd, texture.Width, texture.Height);

            var size = texture.Width * texture.Height;
            uint[] buffer = GetBuffer(size);
            texture.GetData(buffer, 0, size);

            newTexture.SetData(buffer, 0, size);
            return newTexture;
        }

        public static void CopyAlpha(ref Texture2D TextureTo, Texture2D TextureFrom)
        {
            if (TextureTo.Width != TextureFrom.Width || TextureTo.Height != TextureFrom.Height)
            {
                return;
            }


            var size = TextureTo.Width * TextureTo.Height;
            uint[] buffer = GetBuffer(size);
            TextureTo.GetData(buffer, 0, size);

            var sizeFrom = TextureFrom.Width * TextureFrom.Height;
            var bufferFrom = GetBuffer(sizeFrom);
            TextureFrom.GetData(bufferFrom, 0, sizeFrom);

            for (int i = 0; i < size; i++)
            {
                //ARGB
                buffer[i] = (buffer[i] & 0x00FFFFFF) | (bufferFrom[i] & 0xFF000000);
            }

            TextureTo.SetData(buffer, 0, size);
        }

        /// <summary>
        /// Manually replaces a specified color in a texture with transparent black,
        /// thereby masking it.
        /// </summary>
        /// <param name="Texture">The texture on which to apply the mask.</param>
        /// <param name="ColorFrom">The color to mask away.</param>
        public static void ManualTextureMask(ref Texture2D Texture, uint[] ColorsFrom)
        {
            var ColorTo = Color.Transparent.PackedValue;

            //lock (TEXTURE_MASK_BUFFER)
            //{
                
                var size = Texture.Width * Texture.Height;
                uint[] buffer = GetBuffer(size);
                //uint[] buffer = new uint[size];

                //var buffer = TEXTURE_MASK_BUFFER;
                Texture.GetData(buffer, 0, size);

                var didChange = false;

                for (int i = 0; i < size; i++)
                {
                    if (ColorsFrom.Contains(buffer[i]))
                    {
                        didChange = true;
                        buffer[i] = ColorTo;
                    }
                }

                if (didChange)
                {
                    Texture.SetData(buffer, 0, size);
                }
        }

        private static uint[] SINGLE_THREADED_TEXTURE_BUFFER = new uint[MaxResampleBufferSize];
        public static void ManualTextureMaskSingleThreaded(ref Texture2D Texture, uint[] ColorsFrom)
        {
            var ColorTo = Color.Transparent.PackedValue;
            
            var size = Texture.Width * Texture.Height;
            uint[] buffer = new uint[size];

            Texture.GetData<uint>(buffer);

            var didChange = false;

            for (int i = 0; i < size; i++)
            {
                
                if (ColorsFrom.Contains(buffer[i]))
                {
                    didChange = true;
                    buffer[i] = ColorTo;
                }
            }

            if (didChange)
            {
                Texture.SetData(buffer, 0, size);
            }
            else return;
        }

        /// <summary>
        /// Combines multiple textures into a single texture
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="textures"></param>
        /// <returns></returns>
        public static Texture2D MergeHorizontal(GraphicsDevice gd, params Texture2D[] textures)
        {
            return MergeHorizontal(gd, 0, textures);
        }

        /// <summary>
        /// Combines multiple textures into a single texture
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="textures"></param>
        /// <returns></returns>
        public static Texture2D MergeHorizontal(GraphicsDevice gd, int tailPx, params Texture2D[] textures)
        {
            var width = 0;
            var maxHeight = 0;
            var maxWidth = 0;

            foreach (var texture in textures)
            {
                width += texture.Width;
                maxHeight = Math.Max(maxHeight, texture.Height);
                maxWidth = Math.Max(maxWidth, texture.Width);
            }

            width += tailPx;

            Texture2D newTexture = new Texture2D(gd, width, maxHeight);
            Color[] newTextureData = new Color[width * maxHeight];
            Color[] tempTexData = new Color[maxWidth * maxHeight];

            var xo = 0;
            for (var i = 0; i < textures.Length; i++)
            {
                var tx = textures[i];
                tx.GetData<Color>(tempTexData);
                for (var y = 0; y < tx.Height; y++)
                {
                    var yOffset = y * width;

                    for (var x = 0; x < tx.Width; x++)
                    {
                        newTextureData[yOffset + xo + x] = tempTexData[tx.Width * y + x];
                    }
                }
                xo += tx.Width;
            }

            newTexture.SetData(newTextureData);
            tempTexData = null;

            return newTexture;
        }

        public static Texture2D Resize(GraphicsDevice gd, Texture2D texture, int newWidth, int newHeight)
        {
            return texture; //todo: why is this broken (framebuffer incomplete when we try to bind it)

            /*RenderTarget2D renderTarget = new RenderTarget2D(
                gd,
                newWidth, newHeight, false,
                SurfaceFormat.Color, DepthFormat.None);
           
            Rectangle destinationRectangle = new Rectangle(0, 0, newWidth, newHeight);
            lock (gd)
            {
                gd.SetRenderTarget(renderTarget);
                SpriteBatch batch = new SpriteBatch(gd);
                batch.Begin();
                batch.Draw(texture, destinationRectangle, Color.White);
                batch.End();
                gd.SetRenderTarget(null);
            }
            var newTexture = renderTarget;
            return newTexture; */
        }

        public static Texture2D Scale(GraphicsDevice gd, Texture2D texture, float scaleX, float scaleY)
        {
            var newWidth = (int)(texture.Width * scaleX);
            var newHeight = (int)(texture.Height * scaleY);

            RenderTarget2D renderTarget = new RenderTarget2D(
                gd,
                newWidth, newHeight, false,
                SurfaceFormat.Color, DepthFormat.None);

            gd.SetRenderTarget(renderTarget);

            SpriteBatch batch = new SpriteBatch(gd);

            Rectangle destinationRectangle = new Rectangle(0, 0, newWidth, newHeight);

            batch.Begin();
            batch.Draw(texture, destinationRectangle, Color.White);
            batch.End();

            gd.SetRenderTarget(null);

            var newTexture = renderTarget;
            return newTexture;
        }
    }
}
