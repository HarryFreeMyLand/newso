﻿/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Controls;
using System.Timers;
using FSO.HIT;
using FSO.Client.GameContent;
using FSO.Common.Audio;
using FSO.Client.UI.Model;
using System.IO;
using FSO.Files;
using Microsoft.Xna.Framework.Graphics;
using FSO.Common.Utils;
using Microsoft.Xna.Framework;

namespace FSO.Client.UI.Screens
{
    public class LoadingScreen : GameScreen
    {
        private UIContainer BackgroundCtnr;
        private UIImage Background;
        private UILabel ProgressLabel1;
        private UILabel ProgressLabel2;

        private Timer CheckProgressTimer;

        public LoadingScreen()
        {
            HITVM.Get().PlaySoundEvent(UIMusic.LoadLoop);

            /**
             * Scale the whole screen to 1024
             */
            BackgroundCtnr = new UIContainer();
            var scale = ScreenHeight / 600.0f;
            BackgroundCtnr.ScaleX = BackgroundCtnr.ScaleY = scale;

            /** Background image **/
            Texture2D setupTex;
            if (File.Exists("Content/setup.png"))
            {
                using (var logostrm = File.Open("Content/setup.png", FileMode.Open))
                    setupTex = ImageLoader.FromStream(GameFacade.GraphicsDevice, logostrm);
            }
            else setupTex = GetTexture((ulong)FileIDs.UIFileIDs.setup);
            Background = new UIImage(setupTex);
            var bgScale = 600f / setupTex.Height;
            Background.SetSize(setupTex.Width * bgScale, 600);
            Background.X = (800 - bgScale * setupTex.Width) / 2;
            BackgroundCtnr.Add(Background);
            BackgroundCtnr.X = (ScreenWidth - (800 * scale)) / 2;

            Texture2D splashSeg;
            using (var logostrm = File.Open("Content/Textures/splashSeg.png", FileMode.Open))
                splashSeg = ImageLoader.FromStream(GameFacade.GraphicsDevice, logostrm);

            var bgEdge = new UIImage(splashSeg).With9Slice(64, 64, 1, 1);
            BackgroundCtnr.AddAt(0,bgEdge);
            bgEdge.Y = -1;
            bgEdge.X = Background.X-64;
            bgEdge.SetSize(Background.Width+64*2, ScreenHeight + 2);

            //TODO: Letter spacing is a bit wrong on this label
            var lbl = new UILabel();
            lbl.Caption = GameFacade.Strings.GetString("154", "5");
            lbl.X = 0;
            lbl.Size = new Microsoft.Xna.Framework.Vector2(800, 100);
            lbl.Y = 508;

            var style = lbl.CaptionStyle.Clone();
            style.Size = 17;
            lbl.CaptionStyle = style;
            BackgroundCtnr.Add(lbl);
            this.Add(BackgroundCtnr);

            ProgressLabel1 = new UILabel
            {
                X = 0,
                Y = 550,
                Size = new Microsoft.Xna.Framework.Vector2(800, 100),
                CaptionStyle = style
            };

            ProgressLabel2 = new UILabel
            {
                X = 0,
                Y = 550,
                Size = new Microsoft.Xna.Framework.Vector2(800, 100),
                CaptionStyle = style
            };

            BackgroundCtnr.Add(ProgressLabel1);
            BackgroundCtnr.Add(ProgressLabel2);

            PreloadLabels = new string[]{
                GameFacade.Strings.GetString("155", "6"),
                GameFacade.Strings.GetString("155", "7"),
                GameFacade.Strings.GetString("155", "8"),
                GameFacade.Strings.GetString("155", "9")
            };

            CurrentPreloadLabel = 0;
            AnimateLabel("", PreloadLabels[0]);

            CheckProgressTimer = new Timer();
            CheckProgressTimer.Interval = 5;
            CheckProgressTimer.Elapsed += new ElapsedEventHandler(CheckProgressTimer_Elapsed);
            CheckProgressTimer.Start();

            //GameFacade.Screens.Tween.To(rect, 10.0f, new Dictionary<string, float>() {
            //    {"X", 500.0f}
            //}, TweenQuad.EaseInOut);
        }

        void CheckProgressTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckPreloadLabel();
        }

        private string[] PreloadLabels;
        private int CurrentPreloadLabel = 0;
        private bool InTween = false;

        private void CheckPreloadLabel()
        {
            /** Have we preloaded the correct percent? **/
            var percentDone = ContentManager.PreloadProgress;
            var percentUntilNextLabel = ((float)(CurrentPreloadLabel + 1)) / ((float)PreloadLabels.Length);

            //System.Diagnostics.Debug.WriteLine(percentDone);

            if (percentDone >= percentUntilNextLabel)
            {
                if (!InTween)
                {
                    if (CurrentPreloadLabel + 1 < PreloadLabels.Length)
                    {
                        CurrentPreloadLabel++;
                        AnimateLabel(PreloadLabels[CurrentPreloadLabel - 1], PreloadLabels[CurrentPreloadLabel]);
                    }
                    else
                    {
                        /** No more labels to show! Preload must be complete :) **/
                        CheckProgressTimer.Stop();
                        GameFacade.Controller.ShowPersonCreation(new ProtocolAbstractionLibraryD.CityInfo(false));
                    }
                }
            }
        }

        private void AnimateLabel(string previousLabel, string newLabel)
        {
            InTween = true;

            ProgressLabel1.X = 0;
            ProgressLabel1.Caption = previousLabel;

            ProgressLabel2.X = 800;
            ProgressLabel2.Caption = newLabel;

            var tween = GameFacade.Screens.Tween.To(ProgressLabel1, 1.0f, new Dictionary<string, float>() 
            {
                {"X", -800.0f}
            });
            tween.OnComplete += new TweenEvent(tween_OnComplete);

            GameFacade.Screens.Tween.To(ProgressLabel2, 1.0f, new Dictionary<string, float>() 
            {
                {"X", 0.0f}
            });
        }

        private void tween_OnComplete(UITweenInstance tween, float progress)
        {
            InTween = false;
            CheckPreloadLabel();
        }

        public override void Draw(UISpriteBatch batch)
        {
            batch.Draw(TextureGenerator.GetPxWhite(batch.GraphicsDevice), new Rectangle(0, 0, ScreenWidth, ScreenHeight), new Color(0x09,0x18,0x2F));
            base.Draw(batch);
        }
    }
}
