﻿/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using FSO.Client.UI.Framework;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Panels;

namespace FSO.Client.UI.Screens
{
    public class TransitionScreen : GameScreen
    {
        UISetupBackground m_Background;
        UILoginProgress m_LoginProgress;

        /// <summary>
        /// Creates a new CityTransitionScreen.
        /// </summary>
        /// <param name="SelectedCity">The city being transitioned to.</param>
        /// <param name="CharacterCreated">If transitioning from CreateASim, this should be true.
        /// A CharacterCreateCity packet will be sent to the CityServer. Otherwise, this should be false.
        /// A CityToken packet will be sent to the CityServer.</param>
        public TransitionScreen()
        {
            /** Background image **/
            GameFacade.Cursor.SetCursor(Common.Rendering.Framework.CursorType.Hourglass);
            m_Background = new UISetupBackground();

            var lbl = new UILabel
            {
                Caption = "Version " + GlobalSettings.Default.ClientVersion,
                X = 20,
                Y = 558
            };
            m_Background.BackgroundCtnr.Add(lbl);
            Add(m_Background);

            m_LoginProgress = new UILoginProgress();
            m_LoginProgress.X = ScreenWidth - (m_LoginProgress.Width + 20);
            m_LoginProgress.Y = ScreenHeight - (m_LoginProgress.Height + 20);
            m_LoginProgress.Opacity = 0.9f;
            Add(m_LoginProgress);
        }

        public override void GameResized()
        {
            base.GameResized();
            m_LoginProgress.X = ScreenWidth - (m_LoginProgress.Width + 20);
            m_LoginProgress.Y = ScreenHeight - (m_LoginProgress.Height + 20);
        }

        public bool ShowProgress
        {
            get
            {
                return m_LoginProgress.Visible;
            }
            set
            {
                m_LoginProgress.Visible = value;
            }
        }
        
        public void SetProgress(float progress, int stringIndex)
        {
            m_LoginProgress.ProgressCaption = GameFacade.Strings.GetString("251", stringIndex.ToString());
            m_LoginProgress.Progress = progress;
        }
    }
}
