﻿/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Content;
using FSO.Client.UI.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FSO.Client.Rendering;
using FSO.Client.Utils;
using ProtocolAbstractionLibraryD;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Rendering.Framework;
using FSO.Vitaboy;
using FSO.Common.Rendering.Framework.Camera;
using FSO.LotView.Utils;

namespace FSO.Client.UI.Controls
{
    /// <summary>
    /// Renders a sim in the UI, this class just helps translate the UI world
    /// into the 3D world for sim rendering
    /// </summary>
    public class UISim : UIElement
    {
        private _3DTargetScene Scene;
        private WorldCamera Camera;
        public AdultVitaboyModel Avatar;

        /** 45 degrees in either direction **/
        public float RotationRange = 45;
        public float RotationStartAngle = 45;
        public float RotationSpeed = new TimeSpan(0, 0, 10).Ticks;
        public bool AutoRotate = true;

        public float SimScale = 0.45f;
        public float ViewScale = 17.0f;

        private int m_CharacterID;

        protected Guid m_GUID;
        protected string m_Timestamp;
        protected string m_Name;
        protected string m_Sex;
        protected string m_Description;
        protected ulong m_HeadOutfitID;
        protected ulong m_BodyOutfitID;

        //|oft id   |oft type|
        const ulong PROXY_HEAD = 0x000003a00000000D;
        const ulong PROXY_BODY = 0x0000024c0000000D;

        protected int m_LotID = 0;
        protected short m_HouseX, m_HouseY;

        /// <summary>
        /// This sim's lot's ID.
        /// </summary>
        public int LotID
        {
            get { return m_LotID; }
            set { m_LotID = value; }
        }

        /// <summary>
        /// This sim's house's X coordinate on city map.
        /// </summary>
        public short HouseX
        {
            get { return m_HouseX; }
            set { m_HouseX = value; }
        }

        /// <summary>
        /// This sim's house's Y coordinate on city map.
        /// </summary>
        public short HouseY
        {
            get { return m_HouseY; }
            set { m_HouseY = value; }
        }

        public Outfit Head
        {
            get
            {
                if (Avatar.Head == null)
                {
                    try
                    {
                        return Content.GameContent.Get.AvatarOutfits.Get(m_HeadOutfitID);
                    }
                    catch (KeyNotFoundException)
                    {
                        var alert = UIScreen.ShowAlert(new UIAlertOptions { Title = "Error", Message = "Failed to find head with ID: " + m_HeadOutfitID.ToString("X") }, false);
                        return Content.GameContent.Get.AvatarOutfits.Get(PROXY_HEAD);
                    }
                }

                return Avatar.Head;
            }
            set { Avatar.Head = value; }
        }

        public Outfit Body
        {
            get
            {
                if (Avatar.Body == null)
                {
                    try
                    {
                        return Content.GameContent.Get.AvatarOutfits.Get(m_BodyOutfitID);
                    }
                    catch (KeyNotFoundException)
                    {
                        var alert = UIScreen.ShowAlert(new UIAlertOptions { Title = "Error", Message = "Failed to find body with ID: " + m_BodyOutfitID.ToString("X") }, false);
                        return Content.GameContent.Get.AvatarOutfits.Get(PROXY_BODY);
                    }
                }

                return Avatar.Body;
            }

            set { Avatar.Body = value; }
        }

        public Outfit Handgroup
        {
            get { return Avatar.Handgroup; }
            set { Avatar.Handgroup = value; }
        }

        /// <summary>
        /// The ID of the head's outfit. Used by the network protocol.
        /// </summary>
        public ulong HeadOutfitID
        {
            get { return m_HeadOutfitID; }
            set { m_HeadOutfitID = value; }
        }

        /// <summary>
        /// The ID of the body's Outfit. Used by the network protocol.
        /// </summary>
        public ulong BodyOutfitID
        {
            get { return m_BodyOutfitID; }
            set { m_BodyOutfitID = value; }
        }

        protected CityInfo m_City;

        protected bool m_CreatedThisSession = false;

        public float HeadXPos = 0.0f, HeadYPos = 0.0f;

        /// <summary>
        /// This Sim's skeleton.
        /// </summary>
        public Skeleton SimSkeleton
        {
            get
            {
                return Avatar.Skeleton;
            }
        }

        /// <summary>
        /// Received a server-generated GUID.
        /// </summary>
        /// <param name="GUID">The GUID to assign to this sim.</param>
        public void AssignGUID(string GUID)
        {
            m_GUID = new Guid(GUID);
        }

        /// <summary>
        /// A Sim's GUID, created by the client and stored in the DB.
        /// </summary>
        public Guid GUID
        {
            get { return m_GUID; }
        }

        /// <summary>
        /// The character's ID, as it exists in the DB.
        /// </summary>
        public int CharacterID
        {
            get { return m_CharacterID; }
            set { m_CharacterID = value; }
        }

        /// <summary>
        /// When was this character last cached by the client?
        /// </summary>
        public string Timestamp
        {
            get { return m_Timestamp; }
            set { m_Timestamp = value; }
        }

        /// <summary>
        /// The character's name, as it exists in the DB.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public string Sex
        {
            get { return m_Sex; }
            set { m_Sex = value; }
        }

        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        public CityInfo ResidingCity
        {
            get { return m_City; }
            set { m_City = value; }
        }

        /// <summary>
        /// Set to true when a CharacterCreate packet was
        /// received. If this is false, the character in
        /// the DB will NOT be updated with the city that
        /// the character resides in when receiving a 
        /// KeyRequest packet from a CityServer, saving 
        /// an expensive DB call.
        /// </summary>
        public bool CreatedThisSession
        {
            get { return m_CreatedThisSession; }
            set { m_CreatedThisSession = value; }
        }

        private void UISimInit()
        {
            Camera = new WorldCamera(GameFacade.GraphicsDevice);
            Camera.Zoom = LotView.WorldZoom.Near;
            Camera.CenterTile = new Vector2(-1, -1);
            Scene = new _3DTargetScene(GameFacade.Game.GraphicsDevice, Camera, new Point(140, 200), (GlobalSettings.Default.AntiAlias)?8:0);
            Scene.ID = "UISim";

            GameFacade.Game.GraphicsDevice.DeviceReset += new EventHandler<EventArgs>(GraphicsDevice_DeviceReset);

            Avatar = new AdultVitaboyModel();
            Avatar.Scene = Scene;
            
            Scene.Add(Avatar);
        }

        public UISim(string GUID)
        {
            if (GUID != "")
                this.m_GUID = new Guid(GUID);
            UISimInit();
            GameFacade.Scenes.AddExternal(Scene);
        }

        public UISim(string GUID, bool AddScene)
        {
            if ((GUID != "") && (GUID != "\0"))
                this.m_GUID = new Guid(GUID);
            UISimInit();
            if (AddScene)
                GameFacade.Scenes.AddExternal(Scene);
        }

        public UISim(Guid GUID)
        {
            this.m_GUID = GUID;
            UISimInit();
            GameFacade.Scenes.AddExternal(Scene);
        }

        public UISim(Guid GUID, bool AddScene)
        {
            this.m_GUID = GUID;
            UISimInit();
            if (AddScene)
                GameFacade.Scenes.AddExternal(Scene);
        }

        private void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            Scene.DeviceReset(GameFacade.Game.GraphicsDevice);
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            if (AutoRotate)
            {
                var startAngle = RotationStartAngle;
                var time = state.Time.TotalGameTime.Ticks;
                var phase = (time % RotationSpeed) / RotationSpeed;
                var multiplier = Math.Sin((Math.PI * 2) * phase);
                var newAngle = startAngle + (RotationRange * multiplier);
                Avatar.RotationY = (float)MathUtils.DegreeToRadian(newAngle);
            }
        }

        public override void PreDraw(UISpriteBatch batch)
        {
            if (!UISpriteBatch.Invalidated)
            {
                if (!_3DScene.IsInvalidated)
                {
                    batch.Pause();
                    Scene.Draw(GameFacade.GraphicsDevice);
                    batch.Resume();
                    DrawLocalTexture(batch, Scene.Target, new Vector2());
                }
            }
            base.PreDraw(batch);
        }

        public override void Draw(UISpriteBatch batch)
        {
            DrawLocalTexture(batch, Scene.Target, new Vector2());
        }
    }
}
