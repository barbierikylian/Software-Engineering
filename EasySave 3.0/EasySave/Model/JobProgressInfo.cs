using System.ComponentModel;

namespace EasySave.Model
{
    public class JobProgressInfo : INotifyPropertyChanged
    {
        private string _name;
        private int _progress;
        private string _currentFile;
        private string _status;
        private string _playPauseIcon = "⏸";
        private string _playPauseToolTip;
        private string _stopToolTip;
        private bool _isPlayPauseEnabled = true;

        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }
        public int Progress { get { return _progress; } set { _progress = value; OnPropertyChanged("Progress"); } }
        public string CurrentFile { get { return _currentFile; } set { _currentFile = value; OnPropertyChanged("CurrentFile"); } }
        public string Status { get { return _status; } set { _status = value; OnPropertyChanged("Status"); } }
        public string PlayPauseIcon { get { return _playPauseIcon; } set { _playPauseIcon = value; OnPropertyChanged("PlayPauseIcon"); } }
        public string PlayPauseToolTip { get { return _playPauseToolTip; } set { _playPauseToolTip = value; OnPropertyChanged("PlayPauseToolTip"); } }
        public string StopToolTip { get { return _stopToolTip; } set { _stopToolTip = value; OnPropertyChanged("StopToolTip"); } }
        public bool IsPlayPauseEnabled { get { return _isPlayPauseEnabled; } set { _isPlayPauseEnabled = value; OnPropertyChanged("IsPlayPauseEnabled"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}