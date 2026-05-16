using System.ComponentModel;
using EasySave.Model;

namespace EasySave.ViewModel
{
    public class LanguageViewModel : INotifyPropertyChanged
    {
        private LanguageManager _languageManager;

        public string this[string key]
        {
            get
            {
                return _languageManager.GetString(key);
            }
        }

        public string CurrentLanguage
        {
            get
            {
                return _languageManager.GetCurrentLanguage();
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && _languageManager.GetCurrentLanguage() != value)
                {
                    _languageManager.LoadLanguage(value);
                    OnPropertyChanged("CurrentLanguage");
                    OnPropertyChanged("Item[]");
                }
            }
        }

        public LanguageViewModel()
        {
            _languageManager = new LanguageManager();
            _languageManager.LoadLanguage("en");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}