using FSO.Common.DataService.Framework;

namespace FSO.Common.DataService.Model
{
    public class AvatarAppearance : AbstractModel
    {
        ulong _AvatarAppearance_BodyOutfitID;
        public ulong AvatarAppearance_BodyOutfitID
        {
            get { return _AvatarAppearance_BodyOutfitID; }
            set
            {
                _AvatarAppearance_BodyOutfitID = value;
                NotifyPropertyChanged("AvatarAppearance_BodyOutfitID");
            }
        }

        byte _AvatarAppearance_SkinTone;
        public byte AvatarAppearance_SkinTone
        {
            get { return _AvatarAppearance_SkinTone; }
            set
            {
                _AvatarAppearance_SkinTone = value;
                NotifyPropertyChanged("AvatarAppearance_SkinTone");
            }
        }

        ulong _AvatarAppearance_HeadOutfitID;
        public ulong AvatarAppearance_HeadOutfitID
        {
            get { return _AvatarAppearance_HeadOutfitID; }
            set
            {
                _AvatarAppearance_HeadOutfitID = value;
                NotifyPropertyChanged("AvatarAppearance_HeadOutfitID");
            }
        }
    }
}
