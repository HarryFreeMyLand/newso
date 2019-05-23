using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FSO.Common;
using FSO.Common.Rendering.Framework;
using FSO.Common.Utils;
using FSO.Files.RC;
using FSO.LotView;
using FSO.LotView.Facade;
using FSO.Server.Clients;
using FSO.SimAntics;
using FSO.SimAntics.Engine.TSOTransaction;
using FSO.SimAntics.Marshals;
using FSO.SimAntics.Model;
using FSO.SimAntics.NetPlay.Drivers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FSOFacadeWorker
{
    class Program
    {
        static _3DLayer _layer;
        static GraphicsDevice _gd;

        static FacadeConfig _config;

        static void Main(string[] args)
        {
            FSO.Windows.Program.InitWindows();
            TimedReferenceController.SetMode(CacheType.PERMANENT);

            Console.WriteLine("Loading Config...");
            try
            {
                var configString = File.ReadAllText("facadeconfig.json");
                _config = Newtonsoft.Json.JsonConvert.DeserializeObject<FacadeConfig>(configString);
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not find configuration file 'facadeconfig.json'. Please ensure it is valid and present in the same folder as this executable.");
                return;
            }

            Console.WriteLine("Locating The Sims Online...");
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(baseDir);
            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            var os = Environment.OSVersion;
            var pid = os.Platform;
            var linux = pid == PlatformID.MacOSX || pid == PlatformID.Unix;

            bool useDX = true;

            FSOEnvironment.Enable3D = false;
            GameThread.NoGame = true;
            GameThread.UpdateExecuting = true;

            var path = Pathfinder.GamePath;

            if (path != null)
            {
                FSOEnvironment.ContentDir = "Content/";
                FSOEnvironment.GFXContentDir = "Content/" + (useDX ? "DX/" : "OGL/");
                FSOEnvironment.Linux = linux;
                FSOEnvironment.DirectX = useDX;
                FSOEnvironment.GameThread = Thread.CurrentThread;

                FSO.HIT.HITVM.Init();
                FSO.HIT.HITVM.Get.SetMasterVolume(FSO.HIT.Model.HITVolumeGroup.AMBIENCE, 0);
                FSO.HIT.HITVM.Get.SetMasterVolume(FSO.HIT.Model.HITVolumeGroup.FX, 0);
                FSO.HIT.HITVM.Get.SetMasterVolume(FSO.HIT.Model.HITVolumeGroup.MUSIC, 0);
                FSO.HIT.HITVM.Get.SetMasterVolume(FSO.HIT.Model.HITVolumeGroup.VOX, 0);
                FSO.Files.Formats.IFF.Chunks.STR.DefaultLangCode = FSO.Files.Formats.IFF.Chunks.STRLangCode.EnglishUS;
            }

            Console.WriteLine("Creating Graphics Device...");
            var gds = new GraphicsDeviceServiceMock();
            var gd = gds.GraphicsDevice;

            //set up some extra stuff like the content manager
            var services = new GameServiceContainer();
            var content = new ContentManager(services)
            {
                RootDirectory = FSOEnvironment.GFXContentDir
            };
            services.AddService<IGraphicsDeviceService>(gds);

            var vitaboyEffect = content.Load<Effect>("Effects/Vitaboy");
            FSO.Vitaboy.Avatar.setVitaboyEffect(vitaboyEffect);

            WorldConfig.Current = new WorldConfig()
            {
                LightingMode = 3,
                SmoothZoom = true,
                SurroundingLots = 0
            };
            DGRP3DMesh.Sync = true;

            Console.WriteLine("Looks like that worked. Loading FSO Content!");
            // VMContext.InitVMConfig(false);
            // Content.Init(path, gd);
            WorldContent.Init(services, content.RootDirectory);
            // VMAmbientSound.ForceDisable = true;
            _layer = new _3DLayer();
            _layer.Initialize(gd);
            _gd = gd;

            Console.WriteLine("Starting Worker Loop!");
            WorkerLoop();

            Console.WriteLine("Exiting.");
            GameThread.Killed = true;
            GameThread.OnKilled.Set();
            gds.Release();
        }

        public static ApiClient Api;
        public static int TotalLotNum = 0;
        public static List<uint> LotQueue = new List<uint>();
        static bool _loginSent;
        static int _done;

        public static void Login()
        {
            Api = new ApiClient(_config.Api_Url);
            Api.AdminLogin(_config.User, _config.Password, (result) =>
            {
                if (!result)
                {
                    Console.WriteLine("Login Failed! Trying again in a wee bit.");
                    _loginSent = false;
                }
                else
                {
                    Api.GetLotList(1, (lots) =>
                    {
                        Console.WriteLine("Got a lot list for full thumbnail rebake.");
                        //LotQueue.AddRange(lots);
                        //TotalLotNum += lots.Length;
                        //for (int i = 0; i < 4000; i++)
                        //{
                        //    LotQueue.RemoveAt(0);
                        //}
                        RenderLot();
                    });
                }

            });
        }



        static void RenderLot()
        {

            Console.WriteLine("Requesting work...");

            Api.GetWork((shard, location) =>
            {
                if (shard == -1)
                {
                    //no work
                    if (location == uint.MaxValue)
                    {
                        //error, try logging in again
                        _loginSent = false;
                        GameThread.OnWork.Set();
                        return;
                    }
                    else
                    {
                        if (_config.Sleep_Time == 0)
                        {
                            //exit when no work remains
                            Environment.Exit(0);
                            return;
                        }

                        //no work, try again in 30 s
                        Thread.Sleep(_config.Sleep_Time);
                        RenderLot();
                        return;
                    }
                }

                Api.GetFSOV((uint)shard, location, (bt) =>
                {
                    try
                    {
                        if (bt == null)
                        {
                            RenderLot();
                        }
                        else
                        {
                            Console.WriteLine("Rendering lot " + location + "...");
                            var fsof = RenderFSOF(bt, _gd);
                            using (var mem = new MemoryStream())
                            {
                                fsof.Save(mem);
                                Console.WriteLine("Done! Uploading FSOF for lot " + location + ".");

                                //File.WriteAllBytes("C:/fsof/" + lot + ".fsof", mem.ToArray());
                                //RenderLot();
                                Api.UploadFSOF(1, location, mem.ToArray(), (success) =>
                                {
                                    if (!success)
                                    {
                                        Console.WriteLine("Uploading fsof for " + location + " did not succeed.");
                                    }

                                    if (_done++ > _config.Limit)
                                    {
                                        Console.WriteLine("Restarting due to large number of thumbnails rendered.");
                                        Environment.Exit(0);
                                        return;
                                    }
                                    RenderLot();
                                });
                            }
                        }
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("===== Could not render lot " + location + "! =====");
                        Console.WriteLine(e.ToString());
                        RenderLot();
                    }
                });
            });
        }

        public static void WorkerLoop()
        {
            int loggedIn = 0;
            int loginAttempts = 0;
            while (true)
            {
                if (!_loginSent)
                {
                    _loginSent = true;
                    Console.WriteLine("Attempting Login... (" + (loginAttempts++) + ")");
                    Login();
                }
                GameThread.OnWork.WaitOne(1000);
                GameThread.DigestUpdate(null);
            }

        }

        public static FSOF RenderFSOF(byte[] fsov, GraphicsDevice gd)
        {
            var marshal = new VMMarshal();
            using (var mem = new MemoryStream(fsov))
            {
                marshal.Deserialize(new BinaryReader(mem));
            }

            var world = new FSO.LotView.RC.WorldRC(gd);
            world.Opacity = 1;
            _layer.Add(world);

            var globalLink = new VMTSOGlobalLinkStub();
            var driver = new VMServerDriver(globalLink);

            var vm = new VM(new VMContext(world), driver, new VMNullHeadlineProvider());
            vm.Init();

            vm.Load(marshal);

            SetOutsideTime(gd, vm, world, 0.5f, false);
            world.State.PrepareLighting();
            var facade = new LotFacadeGenerator();
            facade.FLOOR_TILES = 64;
            facade.GROUND_SUBDIV = 5;
            facade.FLOOR_RES_PER_TILE = 2;

            SetAllLights(vm, world, 0.5f, 0);

            var result = facade.GetFSOF(gd, world, vm.Context.Blueprint, () => { SetAllLights(vm, world, 0.0f, 100); }, true);

            _layer.Remove(world);
            world.Dispose();
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

            return result;
        }

        static void SetAllLights(VM vm, World world, float outsideTime, short contribution)
        {
            foreach (var light in vm.Entities.Where(x => x.Object.Resource.SemiGlobal?.Iff?.Filename == "lightglobals.iff"))
            {
                light.SetValue(FSO.SimAntics.Model.VMStackObjectVariable.LightingContribution, contribution);
            }
            vm.Context.Architecture.SignalAllDirty();
            vm.Context.Architecture.Tick();
            SetOutsideTime(_gd, vm, world, outsideTime, false);
        }

        static void SetOutsideTime(GraphicsDevice gd, VM vm, World world, float time, bool lightsOn)
        {
            vm.Context.Architecture.SetTimeOfDay(time);
            world.Force2DPredraw(gd);
            vm.Context.Architecture.SetTimeOfDay();
        }
    }
}
