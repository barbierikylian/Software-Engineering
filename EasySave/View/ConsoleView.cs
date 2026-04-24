using System;
using System.Collections.Generic;
using EasySave.Model;
using EasySave.ViewModel;

namespace EasySave.View
{
    public class ConsoleView
    {
        private SaveViewModel saveVM;
        private LanguageViewModel langVM;

        public ConsoleView(SaveViewModel saveViewModel, LanguageViewModel languageViewModel)
        {
            saveVM = saveViewModel;
            langVM = languageViewModel;
        }

        public void Display()
        {
            bool isRunning = true;

            while (isRunning)
            {
                Console.Clear();

                Console.WriteLine("===================================");
                Console.WriteLine(langVM.GetString("welcome"));
                Console.WriteLine("===================================");
                Console.WriteLine("1. " + langVM.GetString("menu_create"));
                Console.WriteLine("2. " + langVM.GetString("menu_execute"));
                Console.WriteLine("3. " + langVM.GetString("menu_list"));
                Console.WriteLine("4. " + langVM.GetString("menu_delete"));
                Console.WriteLine("5. " + langVM.GetString("menu_language"));
                Console.WriteLine("6. " + langVM.GetString("menu_exit"));
                Console.WriteLine("===================================");
                Console.Write(langVM.GetString("choose_option") + " ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1": MenuCreateJob(); break;
                    case "2": MenuExecuteJob(); break;
                    case "3": MenuListJobs(); break;
                    case "4": MenuDeleteJob(); break;
                    case "5": MenuChangeLanguage(); break;
                    case "6":
                        Console.WriteLine(langVM.GetString("goodbye"));
                        isRunning = false;
                        break;
                    default:
                        Console.WriteLine("Option invalide.");
                        break;
                }

                if (isRunning)
                {
                    Console.WriteLine("\nAppuyez sur Entrée pour continuer...");
                    Console.ReadLine();
                }
            }
        }

        private void MenuCreateJob()
        {
            Console.WriteLine("=== Création d'un nouveau job ===");

            if (!saveVM.CanCreateNewJob())
            {
                Console.WriteLine("Erreur : maximum 5 jobs atteint !");
                return;
            }

            Console.Write("Nom du job : ");
            string nom = Console.ReadLine();

            Console.Write("Chemin source : ");
            string source = Console.ReadLine();

            Console.Write("Chemin destination : ");
            string destination = Console.ReadLine();

            Console.Write("Type (Full ou Differential) : ");
            string type = Console.ReadLine();

            saveVM.CreateJob(nom, source, destination, type);
            Console.WriteLine($"Job '{nom}' créé avec succès !");
        }

        private void MenuExecuteJob()
        {
            Console.WriteLine("=== Exécution d'un Job ===");

            List<Backup> jobs = saveVM.GetAllJobs();
            if (jobs.Count == 0)
            {
                Console.WriteLine("Aucun job n'est actuellement configuré.");
                return;
            }

            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {jobs[i].Name} [{jobs[i].Type}]");
            }

            Console.Write("\nEntrez le numéro du job à exécuter (ou appuyez sur Entrée pour tout lancer) : ");
            string input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Exécution de tous les jobs en cours...");
                saveVM.PerformJobs("");
            }
            else if (int.TryParse(input, out int index) && index > 0 && index <= jobs.Count)
            {
                string jobName = jobs[index - 1].Name;
                Console.WriteLine($"Exécution du job '{jobName}' en cours...");
                saveVM.PerformJobs(jobName);
            }
            else
            {
                Console.WriteLine("Saisie invalide. Annulation de l'exécution.");
            }

            Console.WriteLine("Exécution terminée !");
        }

        private void MenuListJobs()
        {
            Console.WriteLine("=== Liste des Jobs configurés ===");

            List<Backup> jobs = saveVM.GetAllJobs();

            if (jobs.Count == 0)
            {
                Console.WriteLine("Aucun job n'est actuellement configuré.");
                return;
            }

            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {jobs[i].Name} [{jobs[i].Type}] : {jobs[i].FileSource} -> {jobs[i].FileDestination}");
            }
        }

        private void MenuDeleteJob()
        {
            Console.WriteLine("=== Suppression d'un Job ===");

            List<Backup> jobs = saveVM.GetAllJobs();
            if (jobs.Count == 0)
            {
                Console.WriteLine("Aucun job n'est actuellement configuré.");
                return;
            }

            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {jobs[i].Name}");
            }

            Console.Write("\nEntrez le numéro du job à supprimer : ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out int index) && index > 0 && index <= jobs.Count)
            {
                string nomASupprimer = jobs[index - 1].Name;
                saveVM.DeleteJob(nomASupprimer);
                Console.WriteLine($"Le job '{nomASupprimer}' a été supprimé.");
            }
            else
            {
                Console.WriteLine("Numéro invalide. Aucune suppression effectuée.");
            }
        }

        private void MenuChangeLanguage()
        {
            Console.WriteLine("=== Changement de langue ===");
            Console.Write("Choisissez la langue (fr/en) : ");
            string lang = Console.ReadLine();

            langVM.UpdateLanguage(lang);
            Console.WriteLine("Langue mise à jour !");
        }
    }
}