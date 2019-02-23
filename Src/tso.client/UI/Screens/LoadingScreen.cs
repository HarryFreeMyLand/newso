﻿/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Controls;
using FSO.HIT;
using FSO.Client.Controllers;
using FSO.Common.Rendering.Framework.Model;
using FSO.Client.UI.Model;
using FSO.Common.Utils;
using Microsoft.Xna.Framework;
using FSO.Client.UI.Panels;
using FSO.Content;

namespace FSO.Client.UI.Screens
{
    public class LoadingScreen : GameScreen
    {
        UISetupBackground Background;
        UILabel ProgressLabel1;
        UILabel ProgressLabel2;

        GameThreadInterval CheckProgressTimer;
        bool PlayedLoadLoop = false;

        public LoadingScreen() : base()
        {

            Background = new UISetupBackground();

            //TODO: Letter spacing is a bit wrong on this label
            var lbl = new UILabel
            {
                Caption = GameFacade.Strings.GetString("154", "5"),
                X = 0,
                Size = new Vector2(800, 100),
                Y = 508
            };

            var style = lbl.CaptionStyle.Clone();
            style.Size = 17;
            lbl.CaptionStyle = style;
            Background.BackgroundCtnr.Add(lbl);
            Add(Background);

            ProgressLabel1 = new UILabel
            {
                X = 0,
                Y = 550,
                Size = new Vector2(800, 100),
                CaptionStyle = style
            };

            ProgressLabel2 = new UILabel
            {
                X = 0,
                Y = 550,
                Size = new Vector2(800, 100),
                CaptionStyle = style
            };

            Background.BackgroundCtnr.Add(ProgressLabel1);
            Background.BackgroundCtnr.Add(ProgressLabel2);

            PreloadLabels = new string[]{
                GameFacade.Strings.GetString("155", "6"),
                GameFacade.Strings.GetString("155", "7"),
                GameFacade.Strings.GetString("155", "8"),
                GameFacade.Strings.GetString("155", "9")
            };

            CurrentPreloadLabel = 0;
            AnimateLabel("", PreloadLabels[0]);


            CheckProgressTimer = GameThread.SetInterval(CheckProgressTimer_Elapsed, 5);

            //GameFacade.Screens.Tween.To(rect, 10.0f, new Dictionary<string, float>() {
            //    {"X", 500.0f}
            //}, TweenQuad.EaseInOut);
        }

        void CheckProgressTimer_Elapsed()
        {
            CheckPreloadLabel();
        }

        string[] PreloadLabels;
        int CurrentPreloadLabel = 0;
        bool InTween = false;

        void CheckPreloadLabel()
        {
            if (Controller == null) { return; }

            /** Have we preloaded the correct percent? **/
            var percentDone = ((LoadingScreenController)Controller).Loader.Progress;
            var percentUntilNextLabel = ((float)(CurrentPreloadLabel + 1)) / ((float)PreloadLabels.Length);

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
                        CheckProgressTimer.Clear();
                        FSOFacade.Controller.ShowLogin();
                        return;
                    }
                }
            }
            if (percentDone >= 1)
            {
                CheckProgressTimer.Clear();
                FSOFacade.Controller.ShowLogin();
                return;
            }
        }

        void AnimateLabel(string previousLabel, string newLabel)
        {
            InTween = true;

            ProgressLabel1.X = 0;
            ProgressLabel1.Caption = previousLabel;

            ProgressLabel2.X = 800;
            ProgressLabel2.Caption = newLabel;

            var tween = GameFacade.Screens.Tween.To(ProgressLabel1, 0.5f, new Dictionary<string, float>()
            {
                {"X", -800.0f}
            });
            tween.OnComplete += new TweenEvent(tween_OnComplete);

            GameFacade.Screens.Tween.To(ProgressLabel2, 0.5f, new Dictionary<string, float>()
            {
                {"X", 0.0f}
            });
        }

        void tween_OnComplete(UITweenInstance tween, float progress)
        {
            InTween = false;
            CheckPreloadLabel();
        }

        public override void Update(UpdateState state)
        {
            if (!PlayedLoadLoop && ((Audio)Content.GameContent.Get.Audio).Initialized)
            {
                HITVM.Get.PlaySoundEvent(UIMusic.LoadLoop);
                PlayedLoadLoop = true;
            }
            base.Update(state);
        }

        public override void Draw(UISpriteBatch batch)
        {
            base.Draw(batch);
        }
    }
}
