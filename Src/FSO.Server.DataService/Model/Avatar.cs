
using FSO.Common.DataService.Framework;
using FSO.Common.DataService.Framework.Attributes;
using System;
using System.Collections.Immutable;

namespace FSO.Common.DataService.Model
{
    public class Avatar : AbstractModel
    {
        [Key]
        public uint Avatar_Id { get; set; }

        public uint FetchTime;

        bool _avatar_IsFounder;
        public bool Avatar_IsFounder
        {
            get { return _avatar_IsFounder; }
            set { _avatar_IsFounder = value; NotifyPropertyChanged("Avatar_IsFounder"); }
        }

        string _Avatar_Name;
        public string Avatar_Name
        {
            get { return _Avatar_Name; }
            set { _Avatar_Name = value; NotifyPropertyChanged("Avatar_Name"); }
        }

        string _Avatar_Description { get; set; }
        [Persist]
        public string Avatar_Description
        {
            get { return _Avatar_Description; }
            set { _Avatar_Description = value; NotifyPropertyChanged("Avatar_Description"); }
        }

        bool _Avatar_IsParentalControlLocked { get; set; }
        public bool Avatar_IsParentalControlLocked
        {
            get { return _Avatar_IsParentalControlLocked; }
            set { _Avatar_IsParentalControlLocked = value; NotifyPropertyChanged("Avatar_IsParentalControlLocked"); }
        }

        byte _Avatar_PrivacyMode { get; set; }
        [Persist]
        public byte Avatar_PrivacyMode
        {
            get { return _Avatar_PrivacyMode; }
            set { _Avatar_PrivacyMode = value; NotifyPropertyChanged("Avatar_PrivacyMode"); }
        }

        ushort _Avatar_CurrentJob { get; set; }
        [ClientSourced]
        public ushort Avatar_CurrentJob
        {
            get { return _Avatar_CurrentJob; }
            set { _Avatar_CurrentJob = value; NotifyPropertyChanged("Avatar_CurrentJob"); }
        }

        #region Loaded from DB on Get

        //These require extra sql queries to set up, so we want to avoid loading these when the user only wants to load
        //a list of avatar names or icons, for example (lot roommates, relationship web).
        //(note that while avatar lot is technically an extra query, it is common enough that we're likely better off
        // loading it immediately in the first place.)
        //It would be a nightmare making these fully async, so they are loaded sync when the user requests them.

        public Func<uint, ImmutableList<JobLevel>> JobLevelProvider;
        ImmutableList<JobLevel> _Avatar_JobLevelVec { get; set; }
        [ClientSourced]
        public ImmutableList<JobLevel> Avatar_JobLevelVec
        {
            get
            {
                if (_Avatar_JobLevelVec == null && JobLevelProvider != null)
                {
                    lock (this)
                    {
                        if (_Avatar_JobLevelVec == null) //lock to prevent getting the same data twice
                            _Avatar_JobLevelVec = JobLevelProvider(this.Avatar_Id);
                    }
                }
                return _Avatar_JobLevelVec;
            }
            set { _Avatar_JobLevelVec = value; NotifyPropertyChanged("Avatar_JobLevelVec"); }
        }

        //todo: client sourced. (incoming relationships will be really tricky)
        public Func<uint, ImmutableList<Relationship>> RelationshipProvider;
        ImmutableList<Relationship> _Avatar_FriendshipVec { get; set; }
        public ImmutableList<Relationship> Avatar_FriendshipVec
        {
            get
            {
                if (_Avatar_FriendshipVec == null && RelationshipProvider != null)
                {
                    lock (this)
                    {
                        if (_Avatar_FriendshipVec == null) //lock to prevent getting the same data twice
                            _Avatar_FriendshipVec = RelationshipProvider(this.Avatar_Id);
                    }
                }
                return _Avatar_FriendshipVec;
            }
            set { _Avatar_FriendshipVec = value; NotifyPropertyChanged("Avatar_FriendshipVec"); }
        }

        public Func<uint, ImmutableList<Bookmark>> BookmarkProvider;
        ImmutableList<Bookmark> _avatar_BookmarksVec;
        [Persist]
        public ImmutableList<Bookmark> Avatar_BookmarksVec
        {
            get
            {
                if (_avatar_BookmarksVec == null && BookmarkProvider != null)
                {
                    lock (this)
                    {
                        if (_avatar_BookmarksVec == null) //lock to prevent getting the same data twice
                            _avatar_BookmarksVec = BookmarkProvider(this.Avatar_Id);
                    }
                }
                return _avatar_BookmarksVec;
            }
            set
            {
                _avatar_BookmarksVec = value;
                NotifyPropertyChanged("Avatar_BookmarksVec");
            }
        }

        #endregion

        bool _Avatar_IsOnline { get; set; }
        public bool Avatar_IsOnline
        {
            get { return _Avatar_IsOnline; }
            set { _Avatar_IsOnline = value; NotifyPropertyChanged("Avatar_IsOnline"); }
        }

        public bool Avatar_IsOffline
        {
            get { return !Avatar_IsOnline; }
        }

        uint _Avatar_LotGridXY;
        public uint Avatar_LotGridXY
        {
            get { return _Avatar_LotGridXY; }
            set { _Avatar_LotGridXY = value; NotifyPropertyChanged("Avatar_LotGridXY"); }
        }

        uint _Avatar_Age;
        public uint Avatar_Age
        {
            get { return _Avatar_Age; }
            set { _Avatar_Age = value; NotifyPropertyChanged("Avatar_Age"); }
        }

        ushort _Avatar_SkillsLockPoints;
        [ClientSourced]
        public ushort Avatar_SkillsLockPoints
        {
            get { return _Avatar_SkillsLockPoints; }
            set { _Avatar_SkillsLockPoints = value; NotifyPropertyChanged("Avatar_SkillsLockPoints"); }
        }

        AvatarAppearance _Avatar_Appearance;
        public AvatarAppearance Avatar_Appearance
        {
            get { return _Avatar_Appearance; }
            set
            {
                _Avatar_Appearance = value;
                NotifyPropertyChanged("Avatar_Appearance");
            }
        }

        AvatarSkills _Avatar_Skills;
        [ClientSourced]
        public AvatarSkills Avatar_Skills
        {
            get { return _Avatar_Skills; }
            set
            {
                _Avatar_Skills = value;
                NotifyPropertyChanged("Avatar_Skills");
            }
        }

        Top100ListFilter _Avatar_Top100ListFilter;
        public Top100ListFilter Avatar_Top100ListFilter
        {
            get { return _Avatar_Top100ListFilter; }
            set
            {
                _Avatar_Top100ListFilter = value;
                NotifyPropertyChanged("Avatar_Top100ListFilter");
            }
        }

        public bool IsDefaultName
        {
            get { return Avatar_Name == "Retrieving..."; }
        }
    }
}
