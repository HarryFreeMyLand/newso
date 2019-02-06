﻿using FSO.Client.UI.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using FSO.Client.UI.Framework;
using FSO.Common.DataService.Model;
using System.Collections.Immutable;
using FSO.Client.Controllers;
using FSO.Common.Utils;
using FSO.Client.Controllers.Panels;

namespace FSO.Client.UI.Panels
{
    public class UIRelationshipDialog : UIDialog
    {
        UIImage InnerBackground;
        UIButton IncomingButton;
        UIButton OutgoingButton;
        UILabel SortLabel;
        UILabel SearchLabel;
        UITextBox SearchBox;

        UIButton SortFriendButton;
        UIButton SortEnemyButton;
        UIButton SortAlmostFriendButton;
        UIButton SortAlmostEnemyButton;
        UIButton SortRoommateButton;

        UISlider ResultsSlider;
        UIButton SliderUpButton;
        UIButton SliderDownButton;

        UILabel FriendLabel;
        UILabel IncomingLabel;

        UIListBox ResultsBox;
        UIPersonButton TargetIcon;

        HashSet<uint> Filter;
        HashSet<uint> Roommates;
        bool OutgoingMode = true;
        ImmutableList<Relationship> Rels;
        Func<Relationship, int> OrderFunction;
        string LastVal = "";
        string LastName = "";

        public UIRelationshipDialog()
            : base(UIDialogStyle.Standard | UIDialogStyle.Close, true)
        {
            this.Caption = GameFacade.Strings.GetString("f106", "10");
            //f_web_inbtn = 0x1972454856DDBAC,
            //f_web_outbtn = 0x3D3AEF0856DDBAC,

            InnerBackground = new UIImage(GetTexture((ulong)0x7A400000001)).With9Slice(13, 13, 13, 13);
            InnerBackground.Position = new Vector2(15, 65);
            InnerBackground.SetSize(510, 230);
            AddAt(3, InnerBackground);

            ResultsBox = new UIListBox
            {
                Columns = new UIListBoxColumnCollection()
            };
            for (int i = 0; i < 3; i++) ResultsBox.Columns.Add(new UIListBoxColumn() { Width = 170 });
            ResultsBox.Position = new Vector2(25, 82);
            ResultsBox.SetSize(510, 230);
            ResultsBox.RowHeight = 40;
            ResultsBox.NumVisibleRows = 6;
            ResultsBox.SelectionFillColor = Color.TransparentBlack;
            Add(ResultsBox);

            var seat = new UIImage(GetTexture(0x19700000002))
            {
                Position = new Vector2(28, 28)
            };
            Add(seat);

            IncomingButton = new UIButton(GetTexture((ulong)0x1972454856DDBAC))
            {
                Position = new Vector2(33, 33),
                Tooltip = GameFacade.Strings.GetString("f106", "12")
            };
            Add(IncomingButton);
            OutgoingButton = new UIButton(GetTexture((ulong)0x3D3AEF0856DDBAC))
            {
                Position = new Vector2(33, 33),
                Tooltip = GameFacade.Strings.GetString("f106", "13")
            };
            Add(OutgoingButton);

            SearchBox = new UITextBox
            {
                Position = new Vector2(550 - 170, 37)
            };
            SearchBox.SetSize(150, 25);
            SearchBox.OnEnterPress += SearchBox_OnEnterPress;
            Add(SearchBox);

            SortLabel = new UILabel
            {
                Caption = GameFacade.Strings.GetString("f106", "1"),
                Position = new Vector2(95, 30)
            };
            SortLabel.CaptionStyle = SortLabel.CaptionStyle.Clone();
            SortLabel.CaptionStyle.Size = 8;
            Add(SortLabel);

            SearchLabel = new UILabel
            {
                Caption = GameFacade.Strings.GetString("f106", "14"),
                Alignment = Framework.TextAlignment.Right,
                Position = new Vector2(550 - 230, 38),
                Size = new Vector2(50, 1)
            };
            Add(SearchLabel);

            SortFriendButton = new UIButton(GetTexture((ulong)0xCE300000001))
            {
                Tooltip = GameFacade.Strings.GetString("f106", "2"),
                Position = new Vector2(95, 47)
            }; //gizmo_friendliestthumb = 0xCE300000001,
            Add(SortFriendButton);

            SortEnemyButton = new UIButton(GetTexture((ulong)0xCE600000001))
            {
                Tooltip = GameFacade.Strings.GetString("f106", "3")
            }; //gizmo_meanestthumb = 0xCE600000001,
            SortEnemyButton.Position = new Vector2(115, 47) + (new Vector2(17 / 2f, 14) - new Vector2(SortEnemyButton.Texture.Width / 8, SortEnemyButton.Texture.Height));
            Add(SortEnemyButton);

            SortAlmostFriendButton = new UIButton(GetTexture((ulong)0x31600000001))
            {
                Tooltip = GameFacade.Strings.GetString("f106", "4")
            }; //gizmo_top100defaultthumb = 0x31600000001,
            SortAlmostFriendButton.Position = new Vector2(135, 47)
                + (new Vector2(17 / 2f, 14) - new Vector2(SortAlmostFriendButton.Texture.Width / 8, SortAlmostFriendButton.Texture.Height));
            Add(SortAlmostFriendButton);

            SortAlmostEnemyButton = new UIButton(GetTexture((ulong)0xCE400000001))
            {
                Tooltip = GameFacade.Strings.GetString("f106", "5")
            }; //gizmo_infamousthumb = 0xCE400000001,
            SortAlmostEnemyButton.Position = new Vector2(155, 47)
                + (new Vector2(17 / 2f, 14) - new Vector2(SortAlmostEnemyButton.Texture.Width / 8, SortAlmostEnemyButton.Texture.Height));
            Add(SortAlmostEnemyButton);

            SortRoommateButton = new UIButton(GetTexture((ulong)0x4B700000001))
            {
                Tooltip = GameFacade.Strings.GetString("f106", "6")
            }; //ucp far zoom
            SortRoommateButton.Position = new Vector2(175, 47)
                + (new Vector2(17 / 2f, 14) - new Vector2(SortRoommateButton.Texture.Width / 8, SortRoommateButton.Texture.Height));
            Add(SortRoommateButton);

            //gizmo_scrollbarimg = 0x31000000001,
            //gizmo_scrolldownbtn = 0x31100000001,
            //gizmo_scrollupbtn = 0x31200000001,

            ResultsSlider = new UISlider
            {
                Orientation = 1,
                Texture = GetTexture(0x31000000001),
                MinValue = 0,
                MaxValue = 2,

                X = 529,
                Y = 72
            };
            ResultsSlider.SetSize(0, 214f);
            Add(ResultsSlider);

            SliderUpButton = new UIButton(GetTexture(0x31200000001))
            {
                Position = new Vector2(526, 65)
            };
            Add(SliderUpButton);
            SliderDownButton = new UIButton(GetTexture(0x31100000001))
            {
                Position = new Vector2(526, 287)
            };
            Add(SliderDownButton);

            ResultsSlider.AttachButtons(SliderUpButton, SliderDownButton, 1f);
            ResultsBox.AttachSlider(ResultsSlider);

            SetSize(560, 320);

            SortFriendButton.OnButtonClick += (btn) => ChangeOrderFunc(OrderFriendly);
            SortEnemyButton.OnButtonClick += (btn) => ChangeOrderFunc(OrderEnemy);
            SortAlmostFriendButton.OnButtonClick += (btn) => ChangeOrderFunc(OrderAlmostFriendly);
            SortAlmostEnemyButton.OnButtonClick += (btn) => ChangeOrderFunc(OrderAlmostEnemy);
            SortRoommateButton.OnButtonClick += (btn) => ChangeOrderFunc(OrderRoommate);

            ChangeOrderFunc(OrderFriendly);

            IncomingButton.OnButtonClick += (btn) => SetOutgoing(false);
            OutgoingButton.OnButtonClick += (btn) => SetOutgoing(true);

            TargetIcon = new UIPersonButton
            {
                FrameSize = UIPersonButtonSize.SMALL,
                Position = new Vector2(72, 35)
            };
            Add(TargetIcon);

            CloseButton.OnButtonClick += CloseButton_OnButtonClick;

            FriendLabel = new UILabel
            {
                Position = new Vector2(35, 292)
            };
            Add(FriendLabel);

            IncomingLabel = new UILabel
            {
                Position = new Vector2(540 - 36, 292),
                Size = new Vector2(1, 1),
                Alignment = TextAlignment.Right
            };
            Add(IncomingLabel);

            SetOutgoing(true);
        }

        void CloseButton_OnButtonClick(UIElement button)
        {
            FindController<RelationshipDialogController>()?.Close();
        }

        void SearchBox_OnEnterPress(UIElement element)
        {
            if (SearchBox.CurrentText.Length < 2)
            {
                HIT.HITVM.Get().PlaySoundEvent(Model.UISounds.Error);
                return;
            }
            FindController<RelationshipDialogController>().Search(SearchBox.CurrentText);
        }

        public void SetRoommates(IEnumerable<uint> roommates)
        {
            if (roommates == null) Roommates = new HashSet<uint>();
            Roommates = new HashSet<uint>(roommates);
            RedrawRels();
        }

        public void SetPersonID(uint id)
        {
            TargetIcon.Position = new Vector2(68, 37);
            TargetIcon.AvatarId = id;
        }

        void SetOutgoing(bool mode)
        {
            IncomingButton.Visible = mode;
            OutgoingButton.Visible = !mode;

            IncomingLabel.Caption = GameFacade.Strings.GetString("f106", mode?"9":"8");
            OutgoingMode = mode;
            RedrawRels();
        }

        void ChangeOrderFunc(Func<Relationship, int> order)
        {
            OrderFunction = order;

            SortFriendButton.Selected = order == OrderFriendly;
            SortEnemyButton.Selected = order == OrderEnemy;
            SortAlmostFriendButton.Selected = order == OrderAlmostFriendly;
            SortAlmostEnemyButton.Selected = order == OrderAlmostEnemy;
            SortRoommateButton.Selected = order == OrderRoommate;

            RedrawRels();
        }

        int OrderFriendly(Relationship rel)
        {
            return -rel.Relationship_LTR;
        }

        int OrderEnemy(Relationship rel)
        {
            return rel.Relationship_LTR;
        }

        int OrderAlmostFriendly(Relationship rel)
        {
            return Math.Abs(60-rel.Relationship_LTR);
        }

        int OrderAlmostEnemy(Relationship rel)
        {
            return Math.Abs((-60) - rel.Relationship_LTR);
        }

        int OrderRoommate(Relationship rel)
        {
            return -rel.Relationship_LTR;
        }

        public void UpdateRelationships(ImmutableList<Relationship> rels)
        {
            Rels = rels;
            FriendLabel.Caption = GameFacade.Strings.GetString("f106", "7", new string[] {
                rels.Count(x => x.Relationship_IsOutgoing && x.Relationship_LTR >= 60).ToString(),
                rels.Count(x => x.Relationship_IsOutgoing && x.Relationship_LTR <= -60).ToString()
            });
            RedrawRels();
        }

        public void RedrawRels()
        {
            if (Rels == null) return;
            IEnumerable<Relationship> query = Rels.Where(x => x.Relationship_IsOutgoing == OutgoingMode).OrderBy(OrderFunction);
            if (OrderFunction == OrderRoommate)
                query = query.Where(x => Roommates?.Contains(x.Relationship_TargetID) == true);
            if (Filter != null) query = query.Where(x => Filter.Contains(x.Relationship_TargetID));

            var oldItems = ResultsBox.Items;
            if (oldItems != null)
            {
                foreach (var row in oldItems)
                {
                    foreach (var column in row.Columns)
                    {
                        ((IDisposable)column)?.Dispose();
                    }
                }
            }

            var rels = query.ToList();
            var items = new List<UIListBoxItem>();

            var core = FindController<CoreGameScreenController>();
            var c = rels.Count;
            for (int i = 0; i < c; i += 3)
            {
                items.Add(new UIListBoxItem(new { },
                    new UIRelationshipElement(rels[i], core),
                    (i + 1 >= c) ? null : new UIRelationshipElement(rels[i + 1], core),
                    (i + 2 >= c) ? null : new UIRelationshipElement(rels[i + 2], core))
                    );
            }

            ResultsBox.Items = items;
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);

            if (LastVal != SearchBox.CurrentText)
            {
                LastVal = SearchBox.CurrentText;
                if (Filter != null)
                {
                    Filter = null;
                    RedrawRels();
                }
            }

            if (LastName != TargetIcon.Tooltip)
            {
                LastName = TargetIcon.Tooltip;
                Caption = GameFacade.Strings.GetString("f106", "11", new string[] { LastName });
            }

            Invalidate(); //for now invalidate friendship web every frame, because its listbox is pretty complicated.
            //buttonseat_transparent = 0x19700000002,
        }

        public void SetFilter(HashSet<uint> filter)
        {
            Filter = filter;
            RedrawRels();
        }
    }

    public class UIRelationshipElement : UIContainer, IDisposable
    {
        UIPersonButton Icon;
        Relationship Rel;
        Microsoft.Xna.Framework.Graphics.Texture2D Indicator;

        public UIRelationshipElement(Relationship rel, object controller)
        {
            Rel = rel;
            Controller = controller;
            if (rel.Relationship_LTR >= 60) Indicator = GetTexture(0xCE300000001);
            else if (rel.Relationship_LTR <= -60) Indicator = GetTexture(0xCE600000001);
        }

        public void Init()
        {
            if (Icon != null) return;
            Icon = new UIPersonButton
            {
                FrameSize = UIPersonButtonSize.LARGE,
                AvatarId = Rel.Relationship_TargetID
            };
            Add(Icon);
        }

        void DrawRel(UISpriteBatch batch, int x, int y, int value)
        {
            double p = (value + 100) / 200.0;
            Color barcol = new Color((byte)(57 * (1 - p)), (byte)(213 * p + 97 * (1 - p)), (byte)(49 * p + 90 * (1 - p)));
            Color bgcol = new Color((byte)(57 * p + 214 * (1 - p)), (byte)(97 * p), (byte)(90 * p));

            var Filler = TextureGenerator.GetPxWhite(batch.GraphicsDevice);
            batch.Draw(Filler, LocalRect(x+1, y+1, 80, 6), new Color(23,38,55));
            batch.Draw(Filler, LocalRect(x, y, 80, 6), bgcol);
            batch.Draw(Filler, LocalRect(x, y, (int)(80 * p), 6), barcol);
            batch.Draw(Filler, LocalRect(x + (int)(80 * p), y, 1, 6), Color.Black);

            var style = TextStyle.DefaultLabel.Clone();
            style.Size = 7;
            style.Shadow = true;

            DrawLocalString(batch, value.ToString(), new Vector2(x + 84, y - 5), style, new Rectangle(0, 0, 1, 1), TextAlignment.Left);
        }

        public override void Draw(UISpriteBatch batch)
        {
            Init(); //init if we haven't been drawn til now
            base.Draw(batch);

            DrawRel(batch, 40, 18, Rel.Relationship_STR);
            DrawRel(batch, 40, 26, Rel.Relationship_LTR);
            if (Indicator != null) DrawLocalTexture(batch, Indicator, new Rectangle(Indicator.Width / 4, 0, Indicator.Width/4, Indicator.Height), new Vector2(142, 17));

            if (Icon.Tooltip != null)
            {
                var style = TextStyle.DefaultLabel;
                DrawLocalString(batch, style.TruncateToWidth(Icon.Tooltip, 120), new Vector2(40, -2), style);
            }
        }

        public void Dispose()
        {
            if (Icon != null) Remove(Icon);
        }
    }
}
