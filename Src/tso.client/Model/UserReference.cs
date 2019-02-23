using FSO.Common.DataService;
using FSO.Common.DataService.Framework;
using FSO.Common.DataService.Model;
using FSO.Common.Enum;
using FSO.Common.Utils;
using FSO.Content.Model;
using Ninject;
using System.Collections.Generic;

namespace FSO.Client.Model
{
    public abstract class UserReference : AbstractModel
    {
        public abstract UserReferenceType Type { get; }

        uint _id;
        ITextureRef _icon;
        string _name = "Retrieving...";
        static Dictionary<uint, UserReference> _cache = new Dictionary<uint, UserReference>();

        public uint Id
        {
            get { return _id; }
            protected set
            {
                _id = value;
                NotifyPropertyChanged("Id");
            }
        }


        public string Name
        {
            get { return _name; }
            protected set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }


        public ITextureRef Icon
        {
            get { return _icon; }
            protected set
            {
                _icon = value;
                NotifyPropertyChanged("Icon");
            }
        }

        public static void ResetCache()
        {
            _cache.Clear();
        }

        public static UserReference Wrap(Avatar avatar)
        {
            return new AvatarUserReference(avatar);
        }

        public static UserReference Of(UserReferenceType type)
        {
            return new BuiltInUserReference(type);
        }

        public static UserReference Of(UserReferenceType type, uint id)
        {
            if (type == UserReferenceType.AVATAR)
            {
                if (_cache.ContainsKey(id))
                {
                    return _cache[id];
                }
                var value = new AvatarUserReference(id);
                _cache[id] = value;
                return value;
            }
            else
            {
                return new BuiltInUserReference(type);
            }
        }
    }

    public class BuiltInUserReference : UserReference
    {
        readonly UserReferenceType _type;

        public BuiltInUserReference(UserReferenceType type)
        {
            _type = type;

            var content = Content.GameContent.Get;
            switch (type)
            {
                case UserReferenceType.EA:
                    Icon = content.UIGraphics.Get(0x00000B0000000001);
                    Name = GameFacade.Strings.GetString("195", "33");
                    break;
                case UserReferenceType.MAXIS:
                    Icon = content.UIGraphics.Get(0x00000B0100000001);
                    Name = GameFacade.Strings.GetString("195", "34");
                    break;
                case UserReferenceType.MOMI:
                    Icon = content.UIGraphics.Get(0x00000B0200000001);
                    Name = "M.O.M.I";
                    break;
                case UserReferenceType.TSO:
                    Icon = content.UIGraphics.Get(0x00000B0300000001);
                    Name = GameFacade.Strings.GetString("195", "35");
                    break;
            }
        }

        public override UserReferenceType Type
        {
            get
            {
                return _type;
            }
        }
    }

    public class AvatarUserReference : UserReference
    {
        Binding<Avatar> _currentAvatar;
        ulong _headOutfitId;

        public AvatarUserReference(uint avatarId) : this()
        {
            FSOFacade.Kernel.Get<IClientDataService>().Get<Avatar>(avatarId).ContinueWith(x =>
            {
                _currentAvatar.Value = x.Result;
            });
        }

        public AvatarUserReference(Avatar avatar) : this()
        {
            _currentAvatar.Value = avatar;
            Name = avatar.Avatar_Name;
            HeadOutfitId = avatar.Avatar_Appearance?.AvatarAppearance_HeadOutfitID ?? 0;
            Id = avatar.Avatar_Id;
        }

        protected AvatarUserReference()
        {
            _currentAvatar = new Binding<Avatar>()
                .WithBinding(this, "Name", "Avatar_Name")
                .WithBinding(this, "HeadOutfitId", "Avatar_Appearance.AvatarAppearance_HeadOutfitID")
                .WithBinding(this, "Id", "Avatar_Id");
        }

        public void Dispose()
        {
            _currentAvatar.Dispose();
        }


        public ulong HeadOutfitId
        {
            get { return _headOutfitId; }
            set
            {
                _headOutfitId = value;
                RefreshHead();
            }
        }

        void RefreshHead()
        {
            var avatar = _currentAvatar.Value;
            if (avatar != null)
            {
                var content = Content.GameContent.Get;
                var outfit = content.AvatarOutfits.Get(_headOutfitId);
                var appearanceId = outfit.GetAppearance((Vitaboy.AppearanceType)avatar.Avatar_Appearance.AvatarAppearance_SkinTone);
                var appearance = content.AvatarAppearances.Get(appearanceId);
                var thumbnail = content.AvatarThumbnails.Get(appearance.ThumbnailID);
                Icon = thumbnail;
            }
            else
            {
                Icon = null;
            }
        }

        public override UserReferenceType Type
        {
            get
            {
                return UserReferenceType.AVATAR;
            }
        }
    }
}
