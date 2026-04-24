/* using System;

namespace EasySave.Model
{
    public class LanguageManager
    {
        private string currentLanguage;

        public LanguageManager()
        {
            currentLanguage = "en";
        }

        public void LoadLanguage(string language)
        {
            currentLanguage = language;
        }

        public string GetString(string key)
        {
            if (currentLanguage == "fr")
            {
                if (key == "welcome") return "Bienvenue dans EasySave";
                if (key == "menu_create") return "Créer un job de sauvegarde";
                if (key == "menu_execute") return "Exécuter un job";
                if (key == "menu_list") return "Lister les jobs";
                if (key == "menu_delete") return "Supprimer un job";
                if (key == "menu_language") return "Changer la langue";
                if (key == "menu_exit") return "Quitter";
                if (key == "choose_option") return "Choisissez une option : ";
                if (key == "job_name") return "Nom du job : ";
                if (key == "source_path") return "Chemin source : ";
                if (key == "destination_path") return "Chemin destination : ";
                if (key == "backup_type") return "Type (1=Complète, 2=Différentielle) : ";
                if (key == "job_created") return "Job créé avec succès";
                if (key == "job_executed") return "Job exécuté avec succès";
                if (key == "error_max_jobs") return "Erreur : 5 jobs maximum atteints";
                if (key == "error_invalid") return "Entrée invalide";
                if (key == "goodbye") return "Au revoir";
            }
            else
            {
                if (key == "welcome") return "Welcome to EasySave";
                if (key == "menu_create") return "Create a backup job";
                if (key == "menu_execute") return "Execute a job";
                if (key == "menu_list") return "List jobs";
                if (key == "menu_delete") return "Delete a job";
                if (key == "menu_language") return "Change language";
                if (key == "menu_exit") return "Quit";
                if (key == "choose_option") return "Choose an option: ";
                if (key == "job_name") return "Job name: ";
                if (key == "source_path") return "Source path: ";
                if (key == "destination_path") return "Destination path: ";
                if (key == "backup_type") return "Type (1=Full, 2=Differential): ";
                if (key == "job_created") return "Job created successfully";
                if (key == "job_executed") return "Job executed successfully";
                if (key == "error_max_jobs") return "Error: 5 jobs maximum reached";
                if (key == "error_invalid") return "Invalid input";
                if (key == "goodbye") return "Goodbye";
            }

            return key;
        }
    }
}

*/


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;

namespace EasySave.Model
{
    public class LanguageManager
    {
        private string currentLanguage;

        private Dictionary<string, string> translations;

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
    }

    
}

