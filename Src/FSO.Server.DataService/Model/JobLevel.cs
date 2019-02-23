using FSO.Common.DataService.Framework;

namespace FSO.Common.DataService.Model
{
    public class JobLevel : AbstractModel
    {
        ushort _JobLevel_JobType;
        public ushort JobLevel_JobType
        {
            get { return _JobLevel_JobType; }
            set
            {
                _JobLevel_JobType = value;
                NotifyPropertyChanged("JobLevel_JobType");
            }
        }
        ushort _JobLevel_JobGrade;
        public ushort JobLevel_JobGrade
        {
            get { return _JobLevel_JobGrade; }
            set
            {
                _JobLevel_JobGrade = value;
                NotifyPropertyChanged("JobLevel_JobGrade");
            }
        }
        uint _JobLevel_JobExperience;
        public uint JobLevel_JobExperience
        {
            get { return _JobLevel_JobExperience; }
            set
            {
                _JobLevel_JobExperience = value;
                NotifyPropertyChanged("JobLevel_JobExperience");
            }
        }
    }
}
