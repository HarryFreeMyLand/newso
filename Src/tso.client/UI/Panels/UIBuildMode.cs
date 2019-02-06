﻿/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FSO.LotView.Model;
using FSO.Common.Rendering.Framework.Model;
using FSO.SimAntics;
using FSO.SimAntics.Entities;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Controls.Catalog;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Panels.LotControls;
using FSO.Client.UI.Model;

namespace FSO.Client.UI.Panels
{
    //TODO: very similar to buy mode... maybe make a them both subclasses of a single abstract "purchase panel" class.

    public class UIBuildMode : UIDestroyablePanel
    {
        public VM vm;
        public VMAvatar SelectedAvatar;

        public UIButton TerrainButton { get; set; }
        public UIButton WaterButton { get; set; }
        public UIButton WallButton { get; set; }
        public UIButton WallpaperButton { get; set; }
        public UIButton StairButton { get; set; }
        public UIButton FireplaceButton { get; set; }

        public UIButton PlantButton { get; set; }
        public UIButton FloorButton { get; set; }
        public UIButton DoorButton { get; set; }
        public UIButton WindowButton { get; set; }
        public UIButton RoofButton { get; set; }
        public UIButton HandButton { get; set; }

        public Texture2D subtoolsBackground { get; set; }
        public Texture2D dividerImage { get; set; }

        public UIImage Background;
        public UIImage Divider;

        public UIImage SubToolBg;
        public UISlider SubtoolsSlider { get; set; }
        public UIButton PreviousPageButton { get; set; } 
        public UIButton NextPageButton { get; set; }

        private Dictionary<UIButton, int> CategoryMap;
        private List<UICatalogElement> CurrentCategory;

        public UICatalog Catalog;
        public UIObjectHolder Holder;
        public UIQueryPanel QueryPanel;
        public UILotControl LotController;
        private VMMultitileGroup BuyItem;

        private int OldSelection = -1;

        public UIBuildMode(UILotControl lotController)
        {
            LotController = lotController;
            Holder = LotController.ObjectHolder;
            QueryPanel = LotController.QueryPanel;

            var script = this.RenderScript("buildpanel" + ((GlobalSettings.Default.GraphicsWidth < 1024) ? "" : "1024") + ".uis");

            Background = new UIImage(GetTexture((GlobalSettings.Default.GraphicsWidth < 1024) ? (ulong)0x000000D800000002 : (ulong)0x0000018300000002));
            Background.Y = 0;
            Background.BlockInput();
            this.AddAt(0, Background);

            Divider = new UIImage(dividerImage);
            Divider.Position = new Vector2(337, 14);
            this.AddAt(1, Divider);

            SubToolBg = new UIImage(subtoolsBackground);
            SubToolBg.Position = new Vector2(336, 5);
            this.AddAt(2, SubToolBg);

            Catalog = new UICatalog((GlobalSettings.Default.GraphicsWidth < 1024) ? 10 : 20);
            Catalog.OnSelectionChange += new CatalogSelectionChangeDelegate(Catalog_OnSelectionChange);
            Catalog.Position = new Vector2(364, 7);
            this.Add(Catalog);

            CategoryMap = new Dictionary<UIButton, int>
            {
                { TerrainButton, 29 },
                { WaterButton, 5 },
                { WallButton, 7 },
                { WallpaperButton, 8 },
                { StairButton, 2 },
                { FireplaceButton, 4 },

                { PlantButton, 3 },
                { FloorButton, 9 },
                { DoorButton, 0 },
                { WindowButton, 1 },
                { RoofButton, 28 },
                { HandButton, 28 },
            };

            TerrainButton.OnButtonClick += ChangeCategory;
            WaterButton.OnButtonClick += ChangeCategory;
            WallButton.OnButtonClick += ChangeCategory;
            WallpaperButton.OnButtonClick += ChangeCategory;
            StairButton.OnButtonClick += ChangeCategory;
            FireplaceButton.OnButtonClick += ChangeCategory;

            PlantButton.OnButtonClick += ChangeCategory;
            FloorButton.OnButtonClick += ChangeCategory;
            DoorButton.OnButtonClick += ChangeCategory;
            WindowButton.OnButtonClick += ChangeCategory;
            RoofButton.OnButtonClick += ChangeCategory;
            HandButton.OnButtonClick += ChangeCategory;

            PreviousPageButton.OnButtonClick += PreviousPage;
            NextPageButton.OnButtonClick += NextPage;
            SubtoolsSlider.MinValue = 0;
            SubtoolsSlider.OnChange += PageSlider;

            Holder.OnPickup += HolderPickup;
            Holder.OnDelete += HolderDelete;
            Holder.OnPutDown += HolderPutDown;
        }

        public void PageSlider(UIElement element)
        {
            var slider = (UISlider)element;
            SetPage((int)Math.Round(slider.Value));
        }

        public void SetPage(int page)
        {
            bool noPrev = (page == 0);
            PreviousPageButton.Disabled = noPrev;

            bool noNext = (page + 1 == Catalog.TotalPages());
            NextPageButton.Disabled = noNext;

            Catalog.SetPage(page);
            if (OldSelection != -1) Catalog.SetActive(OldSelection, true);

            SubtoolsSlider.Value = page;
        }

        public void PreviousPage(UIElement button)
        {
            int page = Catalog.GetPage();
            if (page == 0) return;
            SetPage(page - 1);
        }

        public void NextPage(UIElement button)
        {
            int page = Catalog.GetPage();
            int totalPages = Catalog.TotalPages();
            if (page + 1 == totalPages) return;
            SetPage(page + 1);
        }

        public void ChangeCategory(UIElement elem)
        {
            TerrainButton.Selected = false;
            WaterButton.Selected = false;
            WallButton.Selected = false;
            WallpaperButton.Selected = false;
            StairButton.Selected = false;
            FireplaceButton.Selected = false;

            PlantButton.Selected = false;
            FloorButton.Selected = false;
            DoorButton.Selected = false;
            WindowButton.Selected = false;
            RoofButton.Selected = false;
            HandButton.Selected = false;

            UIButton button = (UIButton)elem;
            button.Selected = true;
            if (!CategoryMap.ContainsKey(button)) return;
            CurrentCategory = UICatalog.Catalog[CategoryMap[button]];
            Catalog.SetCategory(CurrentCategory);

            int total = Catalog.TotalPages();
            OldSelection = -1;

            SubtoolsSlider.MaxValue = total - 1;
            SubtoolsSlider.Value = 0;

            NextPageButton.Disabled = (total == 1);

            if (LotController.CustomControl != null)
            {
                LotController.CustomControl.Release();
                LotController.CustomControl = null;
            }

            PreviousPageButton.Disabled = true;
            return;
        }

        void Catalog_OnSelectionChange(int selection)
        {
            Holder.ClearSelected();
            var item = CurrentCategory[selection];

            if (LotController.ActiveEntity != null && item.Price > LotController.ActiveEntity.TSOState.Budget.Value) {
                HIT.HITVM.Get().PlaySoundEvent(UISounds.Error);
                return;
            }

            if (OldSelection != -1) Catalog.SetActive(OldSelection, false);
            Catalog.SetActive(selection, true);

            if (LotController.CustomControl != null)
            {
                LotController.CustomControl.Release();
                LotController.CustomControl = null;
            }

            if (item.Special != null)
            {
                var res = item.Special.Res;
                var resID = item.Special.ResID;
                QueryPanel.SetInfo(res.GetIcon(resID), res.GetName(resID), res.GetDescription(resID), res.GetPrice(resID));
                QueryPanel.Mode = 1;
                QueryPanel.Tab = 0;
                QueryPanel.Active = true;
                LotController.CustomControl = (UICustomLotControl)Activator.CreateInstance(item.Special.Control, vm, LotController.World, LotController, item.Special.Parameters);
            }
            else
            {
                BuyItem = vm.Context.CreateObjectInstance(item.GUID, LotTilePos.OUT_OF_WORLD, Direction.NORTH, true);
                QueryPanel.SetInfo(LotController.vm, BuyItem.Objects[0], false);
                QueryPanel.Mode = 1;
                QueryPanel.Tab = 0;
                QueryPanel.Active = true;
                Holder.SetSelected(BuyItem);
            }

            OldSelection = selection;
        }

        public override void Destroy()
        {
            //clean up loose ends
            Holder.OnPickup -= HolderPickup;
            Holder.OnDelete -= HolderDelete;
            Holder.OnPutDown -= HolderPutDown;

            if (LotController.CustomControl != null)
            {
                LotController.CustomControl.Release();
                LotController.CustomControl = null;
            }

            if (Holder.Holding != null)
            {
                //delete object that hasn't been placed yet
                //TODO: all holding objects should obviously just be ghosts.
                //Holder.Holding.Group.Delete(vm.Context);
                Holder.ClearSelected();
                QueryPanel.Active = false;
            }
        }

        private void HolderPickup(UIObjectSelection holding, UpdateState state)
        {
            QueryPanel.Mode = 0;
            QueryPanel.Active = true;
            QueryPanel.Tab = 1;
            QueryPanel.SetInfo(LotController.vm, holding.Group.BaseObject, holding.IsBought);
        }
        private void HolderPutDown(UIObjectSelection holding, UpdateState state)
        {
            if (OldSelection != -1)
            {
                if (!holding.IsBought && (state.KeyboardState.IsKeyDown(Keys.LeftShift) || state.KeyboardState.IsKeyDown(Keys.RightShift)))
                {
                    //place another
                    var prevDir = holding.Dir;
                    Catalog_OnSelectionChange(OldSelection);
                    Holder.Holding.Dir = prevDir;
                }
                else
                {
                    Catalog.SetActive(OldSelection, false);
                    OldSelection = -1;
                }
            }
            QueryPanel.Active = false;
        }

        private void HolderDelete(UIObjectSelection holding, UpdateState state)
        {
            if (OldSelection != -1)
            {
                Catalog.SetActive(OldSelection, false);
                OldSelection = -1;
            }
            QueryPanel.Active = false;
        }

        public override void Update(UpdateState state)
        {
            if (QueryPanel.Mode == 0 && QueryPanel.Active)
            {
                if (Opacity > 0) Opacity -= 1f / 20f;
                else
                {
                    Opacity = 0;
                    Visible = false;
                }
            }
            else
            {
                Visible = true;
                if (Opacity < 1) Opacity += 1f / 20f;
                else Opacity = 1;
            }

            if (LotController.ActiveEntity != null) Catalog.Budget = (int)LotController.ActiveEntity.TSOState.Budget.Value;
            base.Update(state);
        }
    }
}
