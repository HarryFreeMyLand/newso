/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using FSO.Common.Rendering.Framework;
using FSO.LotView;
using FSO.HIT;
using FSO.Client.Network;
using FSO.Client.UI;
using FSO.Client.GameContent;
using Ninject;
using FSO.Client.Regulators;
using FSO.Server.Protocol.Voltron.DataService;
using FSO.Common.DataService;
using FSO.Server.DataService.Providers.Client;
using FSO.Common.Domain;
using FSO.Common.Utils;
using FSO.Common;
using Microsoft.Xna.Framework.Audio;
using FSO.HIT.Model;
using FSO.UI.Model;
using FSO.Files.RC;
using FSO.Files.Formats.IFF;
using FSO.SimAntics;
//using System.Windows.Forms;

namespace FSO.Client
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TSOGame : AGame
    {
        public UILayer UiLayer;
        public _3DLayer SceneMgr;

        public TSOGame() : base()
        {
            GameFacade.Game = this;
            //if (GameFacade.DirectX) TimedReferenceController.SetMode(CacheType.PERMANENT);
            Content.RootDirectory = FSOEnvironment.GFXContentDir;
            Graphics.SynchronizeWithVerticalRetrace = true;

            FSOEnvironment.DPIScaleFactor = GlobalSettings.Default.DPIScaleFactor;
            if (!FSOEnvironment.SoftwareDepth)
            {
                Graphics.PreferredBackBufferWidth = (int)(GlobalSettings.Default.GraphicsWidth * FSOEnvironment.DPIScaleFactor);
                Graphics.PreferredBackBufferHeight = (int)(GlobalSettings.Default.GraphicsHeight * FSOEnvironment.DPIScaleFactor);
                //Graphics.PreferMultiSampling = true;
                Graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
                TargetElapsedTime = new TimeSpan(10000000 / GlobalSettings.Default.TargetRefreshRate);
                FSOEnvironment.RefreshRate = GlobalSettings.Default.TargetRefreshRate;
                Graphics.HardwareModeSwitch = false;
                Graphics.ApplyChanges();
            }

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            try
            {
                GameThread.Game = Thread.CurrentThread;
                Thread.CurrentThread.Name = "Game";
            }
            catch
            {
                //fails on android
            }
        }

        bool _newChange = false;
        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            if (_newChange || !GlobalSettings.Default.Windowed)
                return;
            if (Window.ClientBounds.Width == 0 || Window.ClientBounds.Height == 0)
                return;
            _newChange = true;
            var width = Math.Max(1, Window.ClientBounds.Width);
            var height = Math.Max(1, Window.ClientBounds.Height);
            Graphics.PreferredBackBufferWidth = width;
            Graphics.PreferredBackBufferHeight = height;
            Graphics.ApplyChanges();

            GlobalSettings.Default.GraphicsWidth = width;
            GlobalSettings.Default.GraphicsHeight = height;

            _newChange = false;
            if (UiLayer?.CurrentUIScreen == null)
                return;

            UiLayer.SpriteBatch.ResizeBuffer(GlobalSettings.Default.GraphicsWidth, GlobalSettings.Default.GraphicsHeight);
            GlobalSettings.Default.GraphicsWidth = (int)(width / FSOEnvironment.DPIScaleFactor);
            GlobalSettings.Default.GraphicsHeight = (int)(height / FSOEnvironment.DPIScaleFactor);
            UiLayer.CurrentUIScreen.GameResized();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 32;
            var kernel = new StandardKernel(
                new RegulatorsModule(),
                new NetworkModule(),
                new CacheModule()
            );
            FSOFacade.Kernel = kernel;

            var settings = GlobalSettings.Default;
            if (FSOEnvironment.SoftwareDepth)
            {
                settings.GraphicsWidth = (int)(GraphicsDevice.Viewport.Width / FSOEnvironment.DPIScaleFactor);
                settings.GraphicsHeight = (int)(GraphicsDevice.Viewport.Height / FSOEnvironment.DPIScaleFactor);
            }

            //manage settings
            if (settings.LightingMode == -1)
            {
                if (settings.Lighting)
                {
                    if (settings.Shadows3D)
                        settings.LightingMode = 2;
                    else
                        settings.LightingMode = 1;
                }
                else
                    settings.LightingMode = 0;
                settings.Save();
            }

            FeatureLevelTest.UpdateFeatureLevel(GraphicsDevice);
            if (!FSOEnvironment.MSAASupport)
                settings.AntiAlias = false;

            WorldConfig.Current = new WorldConfig()
            {
                LightingMode = settings.LightingMode,
                SmoothZoom = settings.SmoothZoom,
                SurroundingLots = settings.SurroundingLotMode,
                AA = settings.AntiAlias,
            };

            if (!FSOEnvironment.TexCompressSupport)
                settings.TexCompression = 0;
            else if ((settings.TexCompression & 2) == 0)
            {
                settings.TexCompression = 1;
            }
            FSOEnvironment.TexCompress = (!IffFile.RETAIN_CHUNK_DATA) && (settings.TexCompression & 1) > 0;
            //end settings management

            var os = Environment.OSVersion;
            var pid = os.Platform;
            GameFacade.Unix = pid == PlatformID.MacOSX || pid == PlatformID.Unix;

            FSO.Content.GameContent.TS1Hybrid = GlobalSettings.Default.TS1HybridEnable;
            FSO.Content.GameContent.TS1HybridBasePath = GlobalSettings.Default.TS1HybridPath;
            FSO.Content.GameContent.InitBasic(GlobalSettings.Default.StartupPath, GraphicsDevice);
            //VMContext.InitVMConfig();
            base.Initialize();

            GameFacade.GameThread = Thread.CurrentThread;

            SceneMgr = new _3DLayer();
            SceneMgr.Initialize(GraphicsDevice);

            FSOFacade.Controller = kernel.Get<GameController>();
            FSOFacade.Hints = new UI.Hints.UIHintManager();
            GameFacade.Screens = UiLayer;
            GameFacade.Scenes = SceneMgr;
            GameFacade.GraphicsDevice = GraphicsDevice;
            GameFacade.GraphicsDeviceManager = Graphics;
            GameFacade.Emojis = new Common.Rendering.Emoji.EmojiProvider(GraphicsDevice);
            CurLoader.BmpLoaderFunc = Files.ImageLoader.FromStream;
            GameFacade.Cursor = new CursorManager(GraphicsDevice);
            if (!GameFacade.Unix)
                GameFacade.Cursor.Init(FSO.Content.GameContent.Get.GetPath(""), false);

            /** Init any computed values **/
            GameFacade.Init();

            //init audio now
            HITVM.Init();
            var hit = HITVM.Get;
            hit.SetMasterVolume(HITVolumeGroup.FX, GlobalSettings.Default.FXVolume / 10f);
            hit.SetMasterVolume(HITVolumeGroup.MUSIC, GlobalSettings.Default.MusicVolume / 10f);
            hit.SetMasterVolume(HITVolumeGroup.VOX, GlobalSettings.Default.VoxVolume / 10f);
            hit.SetMasterVolume(HITVolumeGroup.AMBIENCE, GlobalSettings.Default.AmbienceVolume / 10f);

            GameFacade.Strings = new ContentStrings();
            FSOFacade.Controller.StartLoading();

            GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.None };

            try
            {
                var audioTest = new SoundEffect(new byte[2], 44100, AudioChannels.Mono); //initialises XAudio.
                audioTest.CreateInstance().Play();
            }
            catch (Exception e)
            {
                FSOProgram.ShowDialog("Failed to initialize audio: \r\n\r\n" + e.StackTrace);
            }

            IsMouseVisible = true;
            IsFixedTimeStep = true;

            WorldContent.Init(Services, Content.RootDirectory);
            DGRP3DMesh.InitRCWorkers();
            if (!(FSOEnvironment.SoftwareKeyboard && FSOEnvironment.SoftwareDepth))
                AddTextInput();
            Screen.Layers.Add(SceneMgr);
            Screen.Layers.Add(UiLayer);
            GameFacade.LastUpdateState = Screen.State;
            //Bind ninject objects
            kernel.Bind<Content.GameContent>().ToConstant(FSO.Content.GameContent.Get);
            kernel.Load(new ClientDomainModule());

            //Have to be eager with this, it sets a singleton instance on itself to avoid packets having
            //to be created using Ninject for performance reasons
            kernel.Get<cTSOSerializer>();
            var ds = kernel.Get<DataService>();
            ds.AddProvider(new ClientAvatarProvider());

            Window.Title = GameConsts.GameName;
            DiscordRpcEngine.Init();

            if (!GlobalSettings.Default.Windowed && !GameFacade.GraphicsDeviceManager.IsFullScreen)
            {
                GameFacade.GraphicsDeviceManager.ToggleFullScreen();
            }
        }

        /// <summary>
        /// Run this instance with GameRunBehavior forced as Synchronous.
        /// </summary>
        public new void Run()
        {
            Run(GameRunBehavior.Synchronous);
        }

        /// <summary>
        /// Only used on desktop targets. Use extensive reflection to AVOID linking on iOS!
        /// </summary>
        void AddTextInput()
        {
            Window.GetType().GetEvent("TextInput")?.AddEventHandler(Window, (EventHandler<TextInputEventArgs>)GameScreen.TextInput);
        }

        void RegainFocus(object sender, EventArgs e)
        {
            GameFacade.Focus = true;
        }

        void LostFocus(object sender, EventArgs e)
        {
            GameFacade.Focus = false;
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            GameThread.Killed = true;
            GameThread.OnKilled.Set();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            Effect vitaboyEffect = null;
            try
            {
                GameFacade.MainFont = new UI.Framework.Font();
                GameFacade.MainFont.AddSize(10, Content.Load<SpriteFont>("Fonts/FreeSO_10px"));
                GameFacade.MainFont.AddSize(12, Content.Load<SpriteFont>("Fonts/FreeSO_12px"));
                GameFacade.MainFont.AddSize(14, Content.Load<SpriteFont>("Fonts/FreeSO_14px"));
                GameFacade.MainFont.AddSize(16, Content.Load<SpriteFont>("Fonts/FreeSO_16px"));

                GameFacade.EdithFont = new UI.Framework.Font();
                GameFacade.EdithFont.AddSize(12, Content.Load<SpriteFont>("Fonts/Trebuchet_12px"));
                GameFacade.EdithFont.AddSize(14, Content.Load<SpriteFont>("Fonts/Trebuchet_14px"));

                vitaboyEffect = Content.Load<Effect>((FSOEnvironment.GLVer == 2) ? "Effects/VitaboyiOS" : "Effects/Vitaboy");
                UiLayer = new UILayer(this, Content.Load<SpriteFont>("Fonts/FreeSO_12px"), Content.Load<SpriteFont>("Fonts/FreeSO_16px"));
            }
            catch (Exception e)
            {
                FSOProgram.ShowDialog($"Content could not be loaded. Make sure that the {GameConsts.GameName} content has been compiled! (ContentSrc/TSOClientContent.mgcb) \r\n\r\n" + e.ToString());
                Exit();
                Environment.Exit(0);
            }

            Vitaboy.Avatar.setVitaboyEffect(vitaboyEffect);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            GameThread.UpdateExecuting = true;
            DiscordRpcEngine.Update();

            if (HITVM.Get != null)
                HITVM.Get.Tick();

            base.Update(gameTime);
            GameThread.UpdateExecuting = false;
        }
    }
}
