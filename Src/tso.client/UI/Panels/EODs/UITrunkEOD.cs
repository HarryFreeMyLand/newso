﻿using System.Collections.Generic;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Framework.Parser;
using FSO.Common.Utils;
using Microsoft.Xna.Framework.Graphics;
using FSO.Vitaboy;
using FSO.SimAntics;

namespace FSO.Client.UI.Panels.EODs
{
    class UITrunkEOD : UIEOD
    {
        bool IsCostumeTrunk;
        Collection TrunkOutfits { get; set; }
        CollectionItem SelectedOutfit { get; set; }
        ulong SelectedOutfitID;
        UICollectionViewer CostumeOptions { get; set; }
        UIScript Script;

        public AppearanceType UserAppearanceType { get; internal set; } = AppearanceType.Light;

        public UIButton AcceptButton { get; set; }
        public UIImage LargeThumbnail { get; set; }
        public UIImage SubpanelBackground { get; set; }

        public UITrunkEOD(UIEODController controller) : base(controller)
        {
            InitUI();
            AddListeners();
            PlaintextHandlers["trunk_fill_UI"] = FillUIHandler;
        }
        public override void OnClose()
        {
            Send("trunk_close_UI", "");
            CloseInteraction();
            base.OnClose();
        }
        void FillUIHandler(string evt, string collectionPath)
        {
            Controller.ShowEODMode(new EODLiveModeOpt
            {
                Buttons = 1,
                Height = EODHeight.Tall,
                Length = EODLength.Full,
                Tips = EODTextTips.None,
                Timer = EODTimer.None,
                Expandable = false
            });
            // get the collection using the directory
            var content = Content.GameContent.Get;
            TrunkOutfits = content.AvatarCollections.Get(collectionPath);

            // get the skin color of the user
            var avatar = (VMAvatar)LotController.ActiveEntity;
            UserAppearanceType = avatar.Avatar.Appearance;

            // added 23.10.18 for skeleton body for Halloween Event
            var substr = collectionPath.Substring(0, 8);
            if (substr == "costumes")
                IsCostumeTrunk = true;

            // setup the collection view
            CostumeOptions.DataProvider = CollectionToDataProvider(TrunkOutfits);
        }
        void InitUI()
        {
            Script = RenderScript("trunkeod.uis");
            // add background image and thumbnail
            SubpanelBackground = Script.Create<UIImage>("SubpanelBackground");
            AddAt(0, SubpanelBackground);
            LargeThumbnail = Script.Create<UIImage>("LargeThumbnail");
            Add(LargeThumbnail);

            CostumeOptions = Script.Create<UICollectionViewer>("BodySkinBrowser");
            CostumeOptions.Init();
            Add(CostumeOptions);
        }
        void CostumeOptionsChangeHandler(UIElement element)
        {
            SelectedOutfit = (CollectionItem)((UIGridViewerItem)CostumeOptions.SelectedItem).Data;
            // if null, it's the skeleton outfit
            if (SelectedOutfit == null)
            {
                if (IsCostumeTrunk)
                    SelectedOutfitID = 6000069312525;
                else
                {
                    // oh no
                    return;
                }
            }
            else
            {
                var costumePurchasable = Content.GameContent.Get.AvatarPurchasables.Get(SelectedOutfit.PurchasableOutfitId);
                SelectedOutfitID = costumePurchasable.OutfitID;
            }
            LargeThumbnail.Texture = ((UIGridViewerItem)CostumeOptions.SelectedItem).Thumb.Get();
        }
        void AddListeners()
        {
            AcceptButton.OnButtonClick += clickedButton => { Send("trunk_wear_costume", SelectedOutfitID + ""); };
            CostumeOptions.OnChange += new ChangeDelegate(CostumeOptionsChangeHandler);
        }
        /*
         * Shamelessly copied from Fso.Client.UI.Screens.PersonSelectionEdit.cs
         */
        List<object> CollectionToDataProvider(Collection collection)
        {
            var dataProvider = new List<object>();
            foreach (var outfit in collection)
            {
                var purchasable = Content.GameContent.Get.AvatarPurchasables.Get(outfit.PurchasableOutfitId);
                Outfit TmpOutfit = Content.GameContent.Get.AvatarOutfits.Get(purchasable.OutfitID);
                Appearance TmpAppearance = Content.GameContent.Get.AvatarAppearances.Get(TmpOutfit.GetAppearance(UserAppearanceType));
                Common.Content.ContentID thumbID = TmpAppearance.ThumbnailID;

                dataProvider.Add(new UIGridViewerItem
                {
                    Data = outfit,
                    Thumb = new Promise<Texture2D>(x => Content.GameContent.Get.AvatarThumbnails.Get(thumbID).Get(GameFacade.GraphicsDevice))
                });
            }
            // special case for skeleton costume added 23.10.18
            if (IsCostumeTrunk)
            {
                var skeletonBodyPurchaseable = Content.GameContent.Get.AvatarPurchasables.Get(6000069312525);
                var skeletonBodyOutfit = Content.GameContent.Get.AvatarOutfits.Get(6000069312525);
                Appearance TempAppearance = Content.GameContent.Get.AvatarAppearances.Get(skeletonBodyOutfit.GetAppearance(UserAppearanceType));
                Common.Content.ContentID thumbnailID = TempAppearance.ThumbnailID;

                dataProvider.Add(new UIGridViewerItem
                {
                    Data = skeletonBodyPurchaseable,
                    Thumb = new Promise<Texture2D>(x => Content.GameContent.Get.AvatarThumbnails.Get(thumbnailID).Get(GameFacade.GraphicsDevice))
                });
            }
            return dataProvider;
        }
    }
}
