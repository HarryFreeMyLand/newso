using FSO.Common.DataService.Framework;
using FSO.Common.DataService.Framework.Attributes;
using FSO.Common.Domain.Realestate;
using FSO.Common.Serialization.Primitives;
using System.Collections.Immutable;

namespace FSO.Common.DataService.Model
{
    public class Lot : AbstractModel
    {
        public int DbId;

        [Key]
        public uint Id { get; set; }

        byte _Lot_NumOccupants;
        public byte Lot_NumOccupants
        {
            get { return _Lot_NumOccupants; }
            set
            {
                _Lot_NumOccupants = value;
                NotifyPropertyChanged("Lot_NumOccupants");
            }
        }

        bool _Lot_IsOnline;
        [Persist] //bit of a hack here... this "persists" through to the city representation. TODO: make not stupid
        public bool Lot_IsOnline
        {
            get { return _Lot_IsOnline; }
            set
            {
                _Lot_IsOnline = value;
                NotifyPropertyChanged("Lot_IsOnline");
            }
        }

        string _Lot_SpotLightText; //FSO specific var
        [Persist] //bit of a hack here... this "persists" through to the city representation. TODO: make not stupid
        public string Lot_SpotLightText
        {
            get { return _Lot_SpotLightText; }
            set
            {
                _Lot_SpotLightText = value;
                NotifyPropertyChanged("Lot_IsSpotlight");
            }
        }

        string _Lot_Name;
        public string Lot_Name
        {
            get { return _Lot_Name; }
            set
            {
                _Lot_Name = value;
                NotifyPropertyChanged("Lot_Name");
            }
        }

        uint _Lot_Price;
        public uint Lot_Price
        {
            get { return _Lot_Price; }
            set
            {
                _Lot_Price = value;
                NotifyPropertyChanged("Lot_Price");
            }
        }

        public uint _Lot_LeaderID;
        public uint Lot_LeaderID
        {
            get { return _Lot_LeaderID; }
            set
            {
                _Lot_LeaderID = value;
                NotifyPropertyChanged("Lot_LeaderID");
            }
        }

        public string _Lot_Description;

        [Persist]
        public string Lot_Description
        {
            get { return _Lot_Description; }
            set
            {
                _Lot_Description = value;
                NotifyPropertyChanged("Lot_Description");
            }
        }


        uint _Lot_LastCatChange;
        public uint Lot_LastCatChange
        {
            get { return _Lot_LastCatChange; }
            set
            {
                _Lot_HoursSinceLastLotCatChange = value;
                _Lot_LastCatChange = value;
                NotifyPropertyChanged("Lot_HoursSinceLastLotCatChange");
            }
        }

        byte _Lot_Category;

        [Persist]
        public byte Lot_Category
        {
            get { return _Lot_Category; }
            set
            {
                _Lot_Category = value;
                NotifyPropertyChanged("Lot_Category");
            }
        }

        uint _Lot_HoursSinceLastLotCatChange;
        public uint Lot_HoursSinceLastLotCatChange
        {
            get{
                return _Lot_HoursSinceLastLotCatChange;
                //var diff = Epoch.Now - Lot_LastCatChange;
                //return (uint)TimeSpan.FromMilliseconds(diff).TotalMilliseconds;
            }
            set{
                _Lot_LastCatChange = value;
                _Lot_HoursSinceLastLotCatChange = value;
            }
        }

        ImmutableList<uint> _Lot_OwnerVec;
        public ImmutableList<uint> Lot_OwnerVec
        {
            get { return _Lot_OwnerVec; }
            set
            {
                _Lot_OwnerVec = value;
                NotifyPropertyChanged("Lot_OwnerVec");
            }
        }

        ImmutableList<uint> _Lot_RoommateVec;
        public ImmutableList<uint> Lot_RoommateVec
        {
            get { return _Lot_RoommateVec; }
            set { _Lot_RoommateVec = value; NotifyPropertyChanged("Lot_RoommateVec"); }
        }

        LotAdmitInfo _Lot_LotAdmitInfo;
        public LotAdmitInfo Lot_LotAdmitInfo
        {
            get { return _Lot_LotAdmitInfo; }
            set { _Lot_LotAdmitInfo = value; NotifyPropertyChanged("Lot_LotAdmitInfo"); }
        }

        uint _Lot_SkillGamemode;
        [Persist]
        public uint Lot_SkillGamemode
        {
            get { return _Lot_SkillGamemode; }
            set { _Lot_SkillGamemode = value; NotifyPropertyChanged("Lot_SkillGamemode"); }
        }

        public Location Lot_Location { get; set; }

        public uint Lot_Location_Packed
        {
            get
            {
                return (Lot_Location == null)?0:MapCoordinates.Pack(Lot_Location.Location_X, Lot_Location.Location_Y);
            }
        }

        [Persist]
        public cTSOGenericData Lot_Thumbnail { get; set; }

        public uint Lot_ThumbnailCheckSum { get; set; }

        public bool IsDefaultName
        {
            get { return Lot_Name == "Retrieving..."; }
        }
    }
}
