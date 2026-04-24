using EasySave.Model;

namespace EasySave.ViewModel
{
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
            // Basculer entre français et anglais
        }

        public string GetString(string key)
        {
            return languageManager.GetString(key);
        }
    }
}