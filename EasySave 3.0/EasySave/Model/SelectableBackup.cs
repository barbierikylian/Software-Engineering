using System.ComponentModel;

namespace EasySave.Model
{
    public class SelectableBackup : INotifyPropertyChanged
    {
        private bool _isSelected;
        private Backup _job;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged("IsSelected"); }
        }

        public Backup Job
        {
            get { return _job; }
            set { _job = value; OnPropertyChanged("Job"); }
        }

        public SelectableBackup(Backup job)
        {
            _job = job;
            _isSelected = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}