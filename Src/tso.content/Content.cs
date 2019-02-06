/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FSO.Files.FAR3;
using Microsoft.Xna.Framework.Graphics;
using FSO.Common.Content;
using FSO.Files;
using FSO.Files.Formats.IFF;
using System.Threading;

namespace FSO.Content
{
    /// <summary>
    /// Content is a singleton responsible for loading data.
    /// </summary>
    public class GameContent
    {
        public static void Init(string basepath, GraphicsDevice device)
        {
            Get = new GameContent(basepath, device);
        }

        public static GameContent Get { get; private set; }

        /**
         * Content Manager
         */
        public string BasePath;
        public string[] AllFiles;
        private readonly GraphicsDevice _device;

        public ChangeManager Changes;

        /// <summary>
        /// Creates a new instance of Content.
        /// </summary>
        /// <param name="basePath">Path to client directory.</param>
        /// <param name="device">A GraphicsDevice instance.</param>
        private GameContent(string basePath, GraphicsDevice device)
        {
            BasePath = basePath;
            _device = device;

            Changes = new ChangeManager();

            UIGraphics = new UIGraphicsProvider(this, _device);
            AvatarMeshes = new AvatarMeshProvider(this, _device);
            AvatarBindings = new AvatarBindingProvider(this);
            AvatarTextures = new AvatarTextureProvider(this, _device);
            AvatarSkeletons = new AvatarSkeletonProvider(this);
            AvatarAppearances = new AvatarAppearanceProvider(this);
            AvatarOutfits = new AvatarOutfitProvider(this);
            AvatarAnimations = new AvatarAnimationProvider(this);
            AvatarPurchasables = new AvatarPurchasables(this);
            AvatarHandgroups = new HandgroupProvider(this, _device);
            AvatarThumbnails = new AvatarThumbnailProvider(this, _device);

            WorldObjects = new WorldObjectProvider(this);
            WorldFloors = new WorldFloorProvider(this);
            WorldWalls = new WorldWallProvider(this);
            WorldObjectGlobals = new WorldGlobalProvider(this);
            WorldCatalog = new WorldObjectCatalog();

            Audio = new Audio(this);
            GlobalTuning = new Tuning(Path.Combine(basePath, "tuning.dat"));

            Init();
        }

        /// <summary>
        /// Initiates loading for world.
        /// </summary>
        public void InitWorld()
        {
            WorldObjects.Init((_device != null));
            WorldObjectGlobals.Init();
            WorldWalls.Init();
            WorldFloors.Init();
            WorldCatalog.Init(this);
        }

        /// <summary>
        /// Setup the content manager so it knows where to find various files.
        /// </summary>
        private void Init()
        {
            /** Scan system for files **/
            var allFiles = new List<string>();
            _ScanFiles(BasePath, allFiles);
            AllFiles = allFiles.ToArray();

            PIFFRegistry.Init("Content/Patch/");
            Archives = new Dictionary<string, FAR3Archive>();
            UIGraphics.Init();
            AvatarMeshes.Init();
            AvatarBindings.Init();
            AvatarTextures.Init();
            AvatarSkeletons.Init();
            AvatarAppearances.Init();
            AvatarOutfits.Init();
            AvatarAnimations.Init();
            Audio.Init();
            AvatarPurchasables.Init();
            AvatarHandgroups.Init();
            AvatarThumbnails.Init();

            InitWorld();
        }

        /// <summary>
        /// Scans a directory for a list of files.
        /// </summary>
        /// <param name="dir">The directory to scan.</param>
        /// <param name="fileList">The list of files to scan for.</param>
        private void _ScanFiles(string dir, List<string> fileList)
        {
            var fullPath = dir;
            var files = Directory.GetFiles(fullPath);
            foreach (var file in files)
            {
                fileList.Add(file.Substring(BasePath.Length));
            }

            var dirs = Directory.GetDirectories(fullPath);
            foreach (var subDir in dirs)
            {
                _ScanFiles(subDir, fileList);
            }
        }

        /// <summary>
        /// Gets a path relative to the client's directory.
        /// </summary>
        /// <param name="path">The path to combine with the client's directory.</param>
        /// <returns>The path combined with the client's directory.</returns>
        public string GetPath(string path)
        {
            return Path.Combine(BasePath, path);
        }

        private Dictionary<string, FAR3Archive> Archives;

        /// <summary>
        /// Gets a resource using a path and ID.
        /// </summary>
        /// <param name="path">The path to the file. If this path is to an archive, assetID can be null.</param>
        /// <param name="assetID">The ID for the resource. Can be null if path doesn't point to an archive.</param>
        /// <returns></returns>
        public Stream GetResource(string path, ulong assetID)
        {
            if (path.EndsWith(".dat"))
            {
                /** Archive **/
                if (!Archives.ContainsKey(path))
                {
                    FAR3Archive newArchive = new FAR3Archive(GetPath(path));
                    Archives.Add(path, newArchive);
                }

                var archive = Archives[path];
                var bytes = archive.GetItemByID(assetID);
                return new MemoryStream(bytes, false);
            }

            if (path.EndsWith(".bmp") || path.EndsWith(".png") || path.EndsWith(".tga"))
                path = "uigraphics/" + path;

            return File.OpenRead(GetPath(path));
        }

        /** World **/
        public WorldObjectProvider WorldObjects;
        public WorldGlobalProvider WorldObjectGlobals;
        public WorldFloorProvider WorldFloors;
        public WorldWallProvider WorldWalls;
        public WorldObjectCatalog WorldCatalog;

        public UIGraphicsProvider UIGraphics;

        /** Avatar **/
        public AvatarMeshProvider AvatarMeshes;
        public AvatarBindingProvider AvatarBindings;
        public AvatarTextureProvider AvatarTextures;
        public AvatarSkeletonProvider AvatarSkeletons;
        public AvatarAppearanceProvider AvatarAppearances;
        public AvatarOutfitProvider AvatarOutfits;
        public AvatarAnimationProvider AvatarAnimations;
        public AvatarPurchasables AvatarPurchasables;
        public HandgroupProvider AvatarHandgroups;
        public AvatarThumbnailProvider AvatarThumbnails;

        /** Audio **/
        public Audio Audio;

        /** GlobalTuning **/
        public Tuning GlobalTuning;
    }
}
