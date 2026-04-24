using EasySave.Model;

namespace EasySave.ViewModel
{
    // MVVM bridge between the View and the LanguageManager.
    public class LanguageViewModel
    {
        private LanguageManager languageManager;

        public LanguageViewModel()
        {
            languageManager = new LanguageManager();
            languageManager.LoadLanguage("en");
        }

        public void UpdateLanguage(string language)
        {
            languageManager.LoadLanguage(language);
        }

        public void SwitchLanguage()
        {
            if (languageManager.GetCurrentLanguage() == "fr")
            {
                languageManager.LoadLanguage("en");
            }
            else
            {
                languageManager.LoadLanguage("fr");
            }
        }

        public string GetString(string key)
        {
            return languageManager.GetString(key);
        }
    }
}