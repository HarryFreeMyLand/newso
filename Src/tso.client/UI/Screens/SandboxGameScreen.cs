using FSO.Client.Diagnostics;
using FSO.Client.Network.Sandbox;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Model;
using FSO.Client.UI.Panels;
using FSO.Client.UI.Panels.WorldUI;
using FSO.Common;
using FSO.Common.Rendering.Framework;
using FSO.Common.Utils;
using FSO.Files.Formats.IFF.Chunks;
using FSO.HIT;
using FSO.LotView;
using FSO.SimAntics;
using FSO.SimAntics.Model.TSOPlatform;
using FSO.SimAntics.Engine.TSOTransaction;
using FSO.SimAntics.NetPlay;
using FSO.SimAntics.NetPlay.Drivers;
using FSO.SimAntics.NetPlay.Model;
using FSO.SimAntics.NetPlay.Model.Commands;
using FSO.SimAntics.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace FSO.Client.UI.Screens
{
    public class SandboxGameScreen : Framework.GameScreen, IGameScreen
    {
        public UIUCP Ucp;
        public UIGameTitle Title;

        public UIContainer WindowContainer;
        public FSOSandboxServer SandServer;
        public FSOSandboxClient SandCli;
        public UISandboxSelector SandSelect;

        Queue<SimConnectStateChange> _stateChanges;

        public UIJoinLotProgress JoinLotProgress;
        public bool Downtown;

        public UILotControl LotControl { get; set; } //world, lotcontrol and vm will be null if we aren't in a lot.
        World _world;
        public VM vm { get; set; }
        public VMNetDriver Driver;
        public uint VisualBudget { get; set; }

        //for TS1 hybrid mode
        public UINeighborhoodSelectionPanel TS1NeighPanel;
        public FAMI ActiveFamily;

        int _zoomLevel;
        int _rotation = 0;

        public bool InLot
        {
            get
            {
                return vm != null;
            }
        }

        public int ZoomLevel
        {
            get
            {
                if (_zoomLevel < 4 && InLot)
                {
                    return 4 - (int)_world.State.Zoom;
                }
                return _zoomLevel;
            }
            set
            {
                value = Math.Max(1, Math.Min(5, value));

                if (value < 4)
                {
                    if (vm == null)
                    {

                    }
                    else
                    {
                        var targ = (WorldZoom)(4 - value); //near is 3 for some reason... will probably revise
                        HITVM.Get.PlaySoundEvent(UIMusic.None);
                        LotControl.Visible = true;
                        _world.Visible = true;
                        Ucp.SetMode(UIUCP.UCPMode.LotMode);
                        LotControl.SetTargetZoom(targ);
                        if (!FSOEnvironment.Enable3D)
                        {
                            if (_zoomLevel != value)
                                vm.Context.World.InitiateSmoothZoom(targ);
                        }
                        _zoomLevel = value;
                    }
                }
                else //open the sandbox mode lot browser
                {
                    SandSelect = new UISandboxSelector();
                    GlobalShowDialog(SandSelect, true);
                }
                Ucp.UpdateZoomButton();
            }
        }


        public int Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
                if (_world != null)
                {
                    switch (_rotation)
                    {
                        case 0:
                            _world.State.Rotation = WorldRotation.TopLeft;
                            break;
                        case 1:
                            _world.State.Rotation = WorldRotation.TopRight;
                            break;
                        case 2:
                            _world.State.Rotation = WorldRotation.BottomRight;
                            break;
                        case 3:
                            _world.State.Rotation = WorldRotation.BottomLeft;
                            break;
                    }
                }
            }
        }

        public sbyte Level
        {
            get
            {
                if (_world == null)
                    return 1;
                else
                    return _world.State.Level;
            }
            set
            {
                if (_world != null)
                {
                    _world.State.Level = value;
                }
            }
        }

        public sbyte Stories
        {
            get
            {
                if (_world == null)
                    return 2;
                return _world.Stories;
            }
        }

        public SandboxGameScreen() : base()
        {
            _stateChanges = new Queue<SimConnectStateChange>();

            Ucp = new UIUCP(this)
            {
                Y = ScreenHeight - 210
            };
            Ucp.SetInLot(false);
            Ucp.UpdateZoomButton();
            Ucp.MoneyText.Caption = "0";// PlayerAccount.Money.ToString();
            Add(Ucp);

            Title = new UIGameTitle();
            Title.SetTitle("");
            Add(Title);

            WindowContainer = new UIContainer();
            Add(WindowContainer);

            if (Content.GameContent.Get.TS1)
            {
                TS1NeighPanel = new UINeighborhoodSelectionPanel(4);
                TS1NeighPanel.OnHouseSelect += (house) =>
                {
                    ActiveFamily = Content.GameContent.Get.Neighborhood.GetFamilyForHouse((short)house);
                    InitializeLot(Path.Combine(Content.GameContent.Get.TS1BasePath, $"UserData/Houses/House{house.ToString().PadLeft(2, '0')}.iff"), false);// "UserData/Houses/House21.iff"
                    Remove(TS1NeighPanel);
                };
                Add(TS1NeighPanel);
            }
        }

        public override void GameResized()
        {
            base.GameResized();
            Title.SetTitle(Title.Label.Caption);
            Ucp.Y = ScreenHeight - 210;
            _world?.GameResized();
            var oldPanel = Ucp.CurrentPanel;
            Ucp.SetPanel(-1);
            Ucp.SetPanel(oldPanel);
        }

        public void Initialize(string propertyName, bool external)
        {
            Title.SetTitle(propertyName);
            GameFacade.CurrentCityName = propertyName;
            ZoomLevel = 1; //screen always starts at near zoom

            JoinLotProgress = new UIJoinLotProgress();
            InitializeLot(propertyName, external);
        }

        int _switchLot = -1;

        public void ChangeSpeedTo(int speed)
        {
            //0 speed is 0x
            //1 speed is 1x
            //2 speed is 3x
            //3 speed is 10x

            if (vm == null)
                return;

            switch (vm.SpeedMultiplier)
            {
                case 0:
                    switch (speed)
                    {
                        case 1:
                            HITVM.Get.PlaySoundEvent(UISounds.SpeedPTo1);
                            break;
                        case 2:
                            HITVM.Get.PlaySoundEvent(UISounds.SpeedPTo2);
                            break;
                        case 3:
                            HITVM.Get.PlaySoundEvent(UISounds.SpeedPTo3);
                            break;
                    }
                    break;
                case 1:
                    switch (speed)
                    {
                        case 0:
                            HITVM.Get.PlaySoundEvent(UISounds.Speed1ToP);
                            break;
                        case 2:
                            HITVM.Get.PlaySoundEvent(UISounds.Speed1To2);
                            break;
                        case 3:
                            HITVM.Get.PlaySoundEvent(UISounds.Speed1To3);
                            break;
                    }
                    break;
                case 3:
                    switch (speed)
                    {
                        case 0:
                            HITVM.Get.PlaySoundEvent(UISounds.Speed2ToP);
                            break;
                        case 1:
                            HITVM.Get.PlaySoundEvent(UISounds.Speed2To1);
                            break;
                        case 3:
                            HITVM.Get.PlaySoundEvent(UISounds.Speed2To3);
                            break;
                    }
                    break;
                case 10:
                    switch (speed)
                    {
                        case 0:
                            HITVM.Get.PlaySoundEvent(UISounds.Speed3ToP);
                            break;
                        case 1:
                            HITVM.Get.PlaySoundEvent(UISounds.Speed3To1);
                            break;
                        case 2:
                            HITVM.Get.PlaySoundEvent(UISounds.Speed3To2);
                            break;
                    }
                    break;
            }

            switch (speed)
            {
                case 0:
                    vm.SpeedMultiplier = 0;
                    break;
                case 1:
                    vm.SpeedMultiplier = 1;
                    break;
                case 2:
                    vm.SpeedMultiplier = 3;
                    break;
                case 3:
                    vm.SpeedMultiplier = 10;
                    break;
            }
        }

        public override void Update(Common.Rendering.Framework.Model.UpdateState state)
        {
            GameFacade.Game.IsFixedTimeStep = vm == null || vm.Ready;

            Visible = _world?.Visible == true && (_world?.State as LotView.RC.WorldStateRC)?.CameraMode != true;
            GameFacade.Game.IsMouseVisible = Visible;

            if (state.NewKeys.Contains(Keys.F1) && state.CtrlDown)
                FSOFacade.Controller.ToggleDebugMenu();

            base.Update(state);
            if (state.NewKeys.Contains(Keys.D1))
                ChangeSpeedTo(1);
            if (state.NewKeys.Contains(Keys.D2))
                ChangeSpeedTo(2);
            if (state.NewKeys.Contains(Keys.D3))
                ChangeSpeedTo(3);
            if (state.NewKeys.Contains(Keys.P))
                ChangeSpeedTo(0);

            if (_world != null)
            {
                //stub smooth zoom?
            }

            lock (_stateChanges)
            {
                while (_stateChanges.Count > 0)
                {
                    var e = _stateChanges.Dequeue();
                    ClientStateChangeProcess(e.State, e.Progress);
                }
            }

            if (_switchLot > 0)
            {

                InitializeLot(Path.Combine(Content.GameContent.Get.TS1BasePath, $"UserData/Houses/House{_switchLot.ToString().PadLeft(2, '0')}.iff"), false);
                _switchLot = -1;
            }
            if (vm != null)
                vm.Update();
        }

        public override void PreDraw(UISpriteBatch batch)
        {
            base.PreDraw(batch);
            vm?.PreDraw();
        }

        public void CleanupLastWorld()
        {
            if (vm == null)
                return;

            //clear our cache too, if the setting lets us do that
            TimedReferenceController.Clear();
            TimedReferenceController.Clear();

            vm.Context.Ambience.Kill();
            foreach (var ent in vm.Entities)
            { //stop object sounds
                var threads = ent.SoundThreads;
                for (int i = 0; i < threads.Count; i++)
                {
                    threads[i].Sound.RemoveOwner(ent.ObjectID);
                }
                threads.Clear();
            }
            vm.CloseNet(VMCloseNetReason.LeaveLot);
            //Driver.OnClientCommand -= VMSendCommand;
            GameFacade.Scenes.Remove(_world);
            _world.Dispose();
            LotControl.Dispose();
            Remove(LotControl);
            Ucp.SetPanel(-1);
            Ucp.SetInLot(false);
            vm.SuppressBHAVChanges();
            vm = null;
            _world = null;
            Driver = null;
            LotControl = null;

            SandServer?.Shutdown();
            SandCli?.Disconnect();
            SandServer = null;
            SandCli = null;
        }

        /*
        void VMSendCommand(byte[] data)
        {
            var controller = FindController<CoreGameScreenController>();

            if (controller != null)
            {
                controller.SendVMMessage(data);
            }
            //TODO: alternate controller for sandbox/standalone mode?
        }

        void VMShutdown(VMCloseNetReason reason)
        {
            var controller = FindController<CoreGameScreenController>();

            if (controller != null)
            {
                controller.HandleVMShutdown(reason);
            }
        }*/

        public void ClientStateChange(int state, float progress)
        {
            lock (_stateChanges)
                _stateChanges.Enqueue(new SimConnectStateChange(state, progress));
        }

        public void ClientStateChangeProcess(int state, float progress)
        {
            switch (state)
            {
                case 2:
                    JoinLotProgress.ProgressCaption = GameFacade.Strings.GetString("211", "27");
                    JoinLotProgress.Progress = 100f * (0.5f + progress * 0.5f);
                    break;
                case 3:
                    GameFacade.Cursor.SetCursor(CursorType.Normal);
                    RemoveDialog(JoinLotProgress);
                    ZoomLevel = 1;
                    Ucp.SetInLot(true);
                    break;
            }
        }

        public void InitializeLot(string lotName, bool external)
        {
            if (lotName == "")
                return;
            var recording = lotName.ToLowerInvariant().EndsWith(".fsor");
            CleanupLastWorld();

            if (FSOEnvironment.Enable3D)
            {
                var rc = new LotView.RC.WorldRC(GameFacade.GraphicsDevice);
                _world = rc;
            }
            else
                _world = new World(GameFacade.GraphicsDevice);
            _world.Opacity = 1;
            GameFacade.Scenes.Add(_world);

            var settings = GlobalSettings.Default;
            var myState = new VMNetAvatarPersistState()
            {
                Name = settings.LastUser,
                DefaultSuits = new VMAvatarDefaultSuits(settings.DebugGender),
                BodyOutfit = settings.DebugBody,
                HeadOutfit = settings.DebugHead,
                PersistID = (uint)new Random().Next(),
                SkinTone = (byte)settings.DebugSkin,
                Gender = (short)(settings.DebugGender ? 0 : 1),
                Permissions = VMTSOAvatarPermissions.Admin,
                Budget = 1000000,
            };

            if (recording)
            {
                var stream = new FileStream(lotName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var rd = new VMFSORDriver(stream);
                Driver = rd;
            }
            else if (external)
            {
                var cd = new VMClientDriver(ClientStateChange);
                SandCli = new FSOSandboxClient();
                cd.OnClientCommand += (msg) => { SandCli.Write(new VMNetMessage(VMNetMessageType.Command, msg)); };
                cd.OnShutdown += (reason) => SandCli.Disconnect();
                SandCli.OnMessage += cd.ServerMessage;
                SandCli.Connect(lotName);
                Driver = cd;

                var dat = new MemoryStream();
                var str = new BinaryWriter(dat);
                myState.SerializeInto(str);
                var ava = new VMNetMessage(VMNetMessageType.AvatarData, dat.ToArray());
                dat.Close();
                SandCli.OnConnectComplete += () =>
                {
                    SandCli.Write(ava);
                };
            }
            else
            {
                var globalLink = new VMTSOGlobalLinkStub
                {
                    Database = new SimAntics.Engine.TSOGlobalLink.VMTSOStandaloneDatabase()
                };
                var sd = new VMServerDriver(globalLink);
                SandServer = new FSOSandboxServer();

                Driver = sd;
                sd.OnDropClient += SandServer.ForceDisconnect;
                sd.OnTickBroadcast += SandServer.Broadcast;
                sd.OnDirectMessage += SandServer.SendMessage;
                SandServer.OnConnect += sd.ConnectClient;
                SandServer.OnDisconnect += sd.DisconnectClient;
                SandServer.OnMessage += sd.HandleMessage;

                SandServer.Start(37564);
            }

            //Driver.OnClientCommand += VMSendCommand;
            //Driver.OnShutdown += VMShutdown;

            vm = new VM(new VMContext(_world), Driver, new UIHeadlineRendererProvider());
            vm.ListenBHAVChanges();
            vm.Init();

            LotControl = new UILotControl(vm, _world);
            AddAt(0, LotControl);

            var time = DateTime.UtcNow;
            var tsoTime = TSOTime.FromUTC(time);

            vm.Context.Clock.Hours = tsoTime.Item1;
            vm.Context.Clock.Minutes = tsoTime.Item2;
            if (_zoomLevel > 3)
            {
                _world.Visible = false;
                LotControl.Visible = false;
            }

            if (IDEHook.IDE != null)
                IDEHook.IDE.StartIDE(vm);

            vm.OnFullRefresh += VMRefreshed;
            vm.OnChatEvent += Vm_OnChatEvent;
            vm.OnEODMessage += LotControl.EODs.OnEODMessage;
            vm.OnRequestLotSwitch += VMLotSwitch;
            vm.OnGenericVMEvent += Vm_OnGenericVMEvent;

            if (!external && !recording)
            {
                if (!Downtown && ActiveFamily != null)
                {
                    ActiveFamily.SelectWholeFamily();
                    vm.TS1State.ActivateFamily(vm, ActiveFamily);
                }
                BlueprintReset(lotName);

                var experimentalTuning = new Common.Model.DynamicTuning(new List<Common.Model.DynTuningEntry> {
                    new Common.Model.DynTuningEntry() { tuning_type = "overfill", tuning_table = 255, tuning_index = 15, value = 200 },
                    new Common.Model.DynTuningEntry() { tuning_type = "overfill", tuning_table = 255, tuning_index = 5, value = 200 },
                    new Common.Model.DynTuningEntry() { tuning_type = "overfill", tuning_table = 255, tuning_index = 6, value = 200 },
                    new Common.Model.DynTuningEntry() { tuning_type = "overfill", tuning_table = 255, tuning_index = 7, value = 200 },
                    new Common.Model.DynTuningEntry() { tuning_type = "overfill", tuning_table = 255, tuning_index = 8, value = 200 },
                    new Common.Model.DynTuningEntry() { tuning_type = "overfill", tuning_table = 255, tuning_index = 9, value = 200 },
                    new Common.Model.DynTuningEntry() { tuning_type = "feature", tuning_table = 0, tuning_index = 0, value = 1 }, //ts1/tso engine animation timings (1.2x faster)
                });
                vm.ForwardCommand(new VMNetTuningCmd { Tuning = experimentalTuning });

                vm.TSOState.PropertyCategory = 255;
                vm.Context.Clock.Hours = 0;
                vm.TSOState.Size = (10) | (3 << 8);
                vm.Context.UpdateTSOBuildableArea();
                var myClient = new VMNetClient
                {
                    PersistID = myState.PersistID,
                    RemoteIP = "local",
                    AvatarState = myState

                };

                var server = (VMServerDriver)Driver;
                server.ConnectClient(myClient);

                GameFacade.Cursor.SetCursor(CursorType.Normal);
                ZoomLevel = 1;
            }
            vm.MyUID = myState.PersistID;
            ZoomLevel = 1;
        }

        public void BlueprintReset(string path)
        {
            string filename = Path.GetFileName(path);
            try
            {
                using (var file = new BinaryReader(File.OpenRead($"{Path.Combine(FSOEnvironment.UserDir, "LocalHouse/")}{filename.Substring(0, filename.Length - 4)}.fsov")))
                {
                    var marshal = new SimAntics.Marshals.VMMarshal();
                    marshal.Deserialize(file);
                    //vm.SendCommand(new VMStateSyncCmd()
                    //{
                    //    State = marshal
                    //});

                    vm.Load(marshal);
                    vm.Reset();
                }
            }
            catch (Exception)
            {
                var floorClip = Rectangle.Empty;
                var offset = new Point();
                var targetSize = 0;

                var isIff = path.EndsWith(".iff");
                short jobLevel = -1;

                try
                {
                    if (isIff)
                        jobLevel = short.Parse(path.Substring(path.Length - 6, 2));
                    else
                        jobLevel = short.Parse(path.Substring(path.IndexOf('0'), 2));
                }
                catch { }

                vm.SendCommand(new VMBlueprintRestoreCmd
                {
                    JobLevel = jobLevel,
                    XMLData = File.ReadAllBytes(path),
                    IffData = isIff,

                    FloorClipX = floorClip.X,
                    FloorClipY = floorClip.Y,
                    FloorClipWidth = floorClip.Width,
                    FloorClipHeight = floorClip.Height,
                    OffsetX = offset.X,
                    OffsetY = offset.Y,
                    TargetSize = targetSize
                });
            }
            vm.Tick();
        }


        void Vm_OnGenericVMEvent(VMEventType type, object data)
        {
            //hmm...
        }

        void VMLotSwitch(uint lotId)
        {
            if ((short)lotId == -1)
            {
                Downtown = false;
                lotId = (uint)ActiveFamily.HouseNumber;
            }
            else
            {
                Downtown = true;
            }
            _switchLot = (int)lotId;
        }

        void Vm_OnChatEvent(VMChatEvent evt)
        {
            if (ZoomLevel < 4)
            {
                Title.SetTitle(LotControl.GetLotTitle());
            }
        }

        void VMRefreshed()
        {
            if (vm == null)
                return;
            LotControl.ActiveEntity = null;
            LotControl.RefreshCut();
        }

        void SaveHouseButton_OnButtonClick(UIElement button)
        {
            if (vm == null)
                return;

            var exporter = new VMWorldExporter();
            exporter.SaveHouse(vm, GameFacade.GameFilePath("housedata/blueprints/house_00.xml"));
            var marshal = vm.Save();
            Directory.CreateDirectory(Path.Combine(FSOEnvironment.UserDir, "LocalHouse/"));
            using (var output = new FileStream(Path.Combine(FSOEnvironment.UserDir, "LocalHouse/house_00.fsov"), FileMode.Create))
            {
                marshal.SerializeInto(new BinaryWriter(output));
            }
            if (vm.GlobalLink != null)
                ((VMTSOGlobalLinkStub)vm.GlobalLink).Database.Save();
        }
    }
}