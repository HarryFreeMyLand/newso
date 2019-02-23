﻿using FSO.Client.UI.Panels;
using FSO.Common.DataService;
using FSO.Common.DataService.Model;
using FSO.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSO.Client.Controllers.Panels
{
    public class BookmarksController : IDisposable
    {
        Network.Network Network;
        IClientDataService DataService;
        UIBookmarks View;
        Binding<Avatar> Binding;
        BookmarkType CurrentType = BookmarkType.AVATAR;

        public BookmarksController(UIBookmarks view, IClientDataService dataService, Network.Network network)
        {
            Network = network;
            DataService = dataService;
            View = view;
            Binding = new Binding<Avatar>().WithMultiBinding(x => { RefreshResults(); }, "Avatar_BookmarksVec");

            Init();
        }

        void Init()
        {
            DataService.Get<Avatar>(Network.MyCharacter).ContinueWith(x =>
            {
                Binding.Value = x.Result;
            });
        }

        public void ChangeType(BookmarkType type)
        {
            CurrentType = type;
            RefreshResults();
        }

        public void RefreshResults()
        {
            var list = new List<BookmarkListItem>();
            if(Binding.Value != null && Binding.Value.Avatar_BookmarksVec != null)
            {
                var bookmarks = Binding.Value.Avatar_BookmarksVec.Where(x => x.Bookmark_Type == (byte)CurrentType).ToList();
                var enriched = DataService.EnrichList<BookmarkListItem, Bookmark, Avatar>(bookmarks, x => x.Bookmark_TargetID, (bookmark, avatar) =>
                {
                    return new BookmarkListItem {
                        Avatar = avatar,
                        Bookmark = bookmark
                    };
                });

                list = enriched;
            }

            View.SetResults(list);
        }

        /**
            var list = new List<BookmarkListItem>();

            if(Binding.Value != null && Binding.Value.Avatar_BookmarksVec != null)
            {
                var bookmarks = Binding.Value.Avatar_BookmarksVec;
                var ids = bookmarks.Select(x => x.Bookmark_TargetID);
                var avatars = 
            }**/




        public void Toggle()
        {
            if (View.Visible)
            {
                Close();
            }
            else
            {
                Show();
            }
        }

        public void Close()
        {
            View.Visible = false;
        }

        public void Show()
        {
            View.Parent.Add(View);
            View.Visible = true;
        }

        public void Dispose()
        {
        }
    }
}
