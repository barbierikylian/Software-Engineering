using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.Model
{
    public class LanguageManager
    {
        private string currentLanguage = "en";
        private Dictionary<string, string> translations = new Dictionary<string, string>();

        public void LoadLanguage(string language)
        {
            currentLanguage = language;

            string filePath = "Languages/" + language + ".json";
            string jsonContent = File.ReadAllText(filePath);

            translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
        }

        public string GetString(string key)
        {
            return translations[key];
        }

        public string GetCurrentLanguage()
        {
            return currentLanguage;
        }
    }
}