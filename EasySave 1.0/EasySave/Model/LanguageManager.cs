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
            string filePath = Path.Combine(AppContext.BaseDirectory, "Languages", language + ".json");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Language file not found");
            }

            currentLanguage = language;
            string jsonContent = File.ReadAllText(filePath);
            translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent) ?? new();
        }

        public string GetString(string key)
        {
            if (translations.ContainsKey(key))
            {
                return translations[key];
            }
            return "[" + key + "]";
        }

        public string GetCurrentLanguage()
        {
            return currentLanguage;
        }
    }
}