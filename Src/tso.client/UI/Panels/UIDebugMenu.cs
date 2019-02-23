﻿using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Screens;
using FSO.Common.Rendering.Framework.Model;
using FSO.Server.Clients;
using Ninject;
using System.Diagnostics;
using FSO.SimAntics.NetPlay.Model.Commands;

namespace FSO.Client.UI.Panels
{
    public class UIDebugMenu : UIDialog
    {
        UIImage Background;
        UIButton ContentBrowserBtn;

        public UIDebugMenu() : base(UIDialogStyle.Tall, true)
        {
            SetSize(500, 300);
            Caption = "Debug Tools";

            Position = new Microsoft.Xna.Framework.Vector2(
                (GlobalSettings.Default.GraphicsWidth / 2.0f) - 250,
                (GlobalSettings.Default.GraphicsHeight / 2.0f) - 150
            );

            Add(new UIImage()
            {
                Texture = GetTexture(0x00000Cbfb00000001),
                Position = new Microsoft.Xna.Framework.Vector2(40, 95)
            });

            ContentBrowserBtn = new UIButton
            {
                Caption = "Browse Content",
                Position = new Microsoft.Xna.Framework.Vector2(160, 50),
                Width = 300
            };
            ContentBrowserBtn.OnButtonClick += x =>
            {
                //ShowTool(new ContentBrowser());
            };
            Add(ContentBrowserBtn);

            var connectLocalBtn = new UIButton
            {
                Caption = GlobalSettings.Default.UseCustomServer ? "Use default server (TSO)" : "Use custom defined server",
                Position = new Microsoft.Xna.Framework.Vector2(160, 90),
                Width = 300
            };
            connectLocalBtn.OnButtonClick += x =>
            {
                GlobalSettings.Default.UseCustomServer = !GlobalSettings.Default.UseCustomServer;
                connectLocalBtn.Caption = GlobalSettings.Default.UseCustomServer ? "Use default server (TSO)" : "Use custom defined server";
                GlobalSettings.Default.Save();
            };
            Add(connectLocalBtn);

            var cityPainterBtn = new UIButton
            {
                Caption = "Toggle City Painter",
                Position = new Microsoft.Xna.Framework.Vector2(160, 130),
                Width = 300
            };
            cityPainterBtn.OnButtonClick += x =>
            {
                var core = GameFacade.Screens.CurrentUIScreen as CoreGameScreen;
                if (core == null) return;
                if (core.CityRenderer.Plugin == null)
                {
                    core.CityRenderer.Plugin = new Rendering.City.Plugins.MapPainterPlugin(core.CityRenderer);
                    cityPainterBtn.Caption = "Disable City Painter";
                }
                else
                {
                    core.CityRenderer.Plugin = null;
                    cityPainterBtn.Caption = "Enable City Painter";
                }
            };
            Add(cityPainterBtn);

            var benchmarkBtn = new UIButton
            {
                Caption = "VM Performance Benchmark (100k ticks)",
                Position = new Microsoft.Xna.Framework.Vector2(160, 170),
                Width = 300
            };
            benchmarkBtn.OnButtonClick += x =>
            {
                var core = GameFacade.Screens.CurrentUIScreen as IGameScreen;
                if (core == null || core.vm == null)
                {
                    UIScreen.GlobalShowAlert(new UIAlertOptions()
                    {
                        Message = "A VM must be running to benchmark performance."
                    }, true);
                    return;
                }
                var watch = new Stopwatch();
                watch.Start();

                var vm = core.vm;
                var tick = vm.Scheduler.CurrentTickID + 1;
                for (int i=0; i<100000; i++)
                {
                    vm.InternalTick(tick++);
                }

                watch.Stop();

                UIScreen.GlobalShowAlert(new UIAlertOptions()
                {
                    Message = "Ran 100k ticks in "+watch.ElapsedMilliseconds+"ms."
                }, true);
            };
            Add(benchmarkBtn);

            var resyncBtn = new UIButton
            {
                Caption = "Force Resync",
                Position = new Microsoft.Xna.Framework.Vector2(160, 210),
                Width = 300
            };
            resyncBtn.OnButtonClick += x =>
            {
                var core = GameFacade.Screens.CurrentUIScreen as IGameScreen;
                if (core == null || core.vm == null)
                {
                    UIScreen.GlobalShowAlert(new UIAlertOptions()
                    {
                        Message = "A VM must be running to force a resync."
                    }, true);
                    return;
                }
                core.vm.SendCommand(new VMRequestResyncCmd());
            };
            Add(resyncBtn);

            serverNameBox = new UITextBox
            {
                X = 50,
                Y = 300 - 54
            };
            serverNameBox.SetSize(500 - 100, 25);
            serverNameBox.CurrentText = GlobalSettings.Default.GameEntryUrl;

            Add(serverNameBox);
        }
        UITextBox serverNameBox;

        public override void Update(UpdateState state)
        {
            base.Update(state);
            if (state.NewKeys.Contains(Microsoft.Xna.Framework.Input.Keys.M))
            {
                //temporary until data service can inform people they're mod
                //now i know what you're thinking - but these requests are permission checked server side anyways
                GameFacade.EnableMod = true;
            }

            if (serverNameBox.CurrentText != GlobalSettings.Default.GameEntryUrl)
            {
                GlobalSettings.Default.GameEntryUrl = serverNameBox.CurrentText;
                GlobalSettings.Default.CitySelectorUrl = serverNameBox.CurrentText;
                var auth = FSOFacade.Kernel.Get<AuthClient>();
                auth.SetBaseUrl(serverNameBox.CurrentText);
                var city = FSOFacade.Kernel.Get<CityClient>();
                city.SetBaseUrl(serverNameBox.CurrentText);
                GlobalSettings.Default.Save();
            }
        }
    }
}