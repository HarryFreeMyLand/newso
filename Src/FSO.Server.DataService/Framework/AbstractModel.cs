using System.ComponentModel;

namespace FSO.Common.DataService.Framework
{
    [DataServiceModel]
    public abstract class AbstractModel : INotifyPropertyChanged, IModel
    {
        public bool ClientSourced;
        public bool RequestDefaultData
        {
            get; set;
        } = false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
