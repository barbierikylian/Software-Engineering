using System;
using System.Collections.Generic;
using EasySave.Model;
using EasySave.ViewModel;

namespace EasySave.View
{
    // Console UI: displays menus and dispatches user actions to the ViewModels.
    public class ConsoleView
    {
        private SaveViewModel saveVM;
        private LanguageViewModel langVM;

        public ConsoleView(SaveViewModel saveViewModel, LanguageViewModel languageViewModel)
        {
            saveVM = saveViewModel;
            langVM = languageViewModel;
        }

        // Main loop: shows the menu until the user picks "Quit".
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

                try
                {
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
                            Console.WriteLine(langVM.GetString("error_invalid_option"));
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("\n" + langVM.GetString("action_cancelled"));
                }

                if (isRunning)
                {
                    Console.WriteLine("\n" + langVM.GetString("press_enter"));
                    Console.ReadLine();
                }
            }
        }

        private void MenuCreateJob()
        {
            Console.WriteLine("=== " + langVM.GetString("create_title") + " ===");
            Console.WriteLine("(" + langVM.GetString("exit_hint") + ")");

            if (!saveVM.CanCreateNewJob())
            {
                Console.WriteLine(langVM.GetString("error_max_jobs"));
                return;
            }

            Console.Write(langVM.GetString("label_name") + " : ");
            string nom = ReadInputOrCancel();

            Console.Write(langVM.GetString("label_source") + " : ");
            string source = ReadInputOrCancel();

            Console.Write(langVM.GetString("label_dest") + " : ");
            string destination = ReadInputOrCancel();

            Console.Write(langVM.GetString("label_type") + " : ");
            string type = ReadInputOrCancel();

            saveVM.CreateJob(nom, source, destination, type);
            Console.WriteLine(langVM.GetString("success_create").Replace("{nom}", nom));
        }

        private void MenuListJobs()
        {
            Console.WriteLine("=== " + langVM.GetString("list_title") + " ===");

            List<Backup> jobs = saveVM.GetAllJobs();

            if (jobs.Count == 0)
            {
                Console.WriteLine(langVM.GetString("error_no_jobs"));
                return;
            }

            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {jobs[i].Name} [{jobs[i].Type}] : {jobs[i].FileSource} -> {jobs[i].FileDestination}");
            }
        }

        // Accepts empty (= all jobs), "1;3;4" style (= selection) or "exit" (= cancel).
        private void MenuExecuteJob()
        {
            Console.WriteLine("\n=== " + langVM.GetString("execute_title") + " ===");
            List<Backup> jobs = saveVM.GetAllJobs();

            if (jobs.Count == 0)
            {
                Console.WriteLine(langVM.GetString("error_no_jobs"));
                return;
            }
            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {jobs[i].Name} ({jobs[i].FileSource} -> {jobs[i].FileDestination})");
            }

            Console.WriteLine("\n--- " + langVM.GetString("instructions_title") + " ---");
            Console.WriteLine(langVM.GetString("hint_all") + " : [Entrée]");
            Console.WriteLine(langVM.GetString("hint_multiple") + " : 1;3;4");
            Console.WriteLine(langVM.GetString("exit_hint"));

            Console.Write("\n" + langVM.GetString("prompt_execute") + " ");
            string input = ReadInputOrCancel();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine(">>> " + langVM.GetString("executing_all"));
                saveVM.PerformJobs("");
            }
            else if (input.ToLower() == "q")
            {
                return;
            }
            else
            {
                var parts = input.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> selectedJobs = new List<string>();

                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int index) && index > 0 && index <= jobs.Count)
                    {
                        selectedJobs.Add(jobs[index - 1].Name);
                    }
                    else
                    {
                        Console.WriteLine($"[!] {langVM.GetString("error_invalid_index")}: {part}");
                    }
                }

                if (selectedJobs.Count > 0)
                {
                    Console.WriteLine($"\n>>> {langVM.GetString("executing_selection")} : {string.Join(", ", selectedJobs)}");
                    foreach (var jobName in selectedJobs)
                    {
                        Console.WriteLine($"\n[En cours : {jobName}]");
                        saveVM.PerformJobs(jobName);
                    }
                }
                else
                {
                    Console.WriteLine(langVM.GetString("error_no_valid_selection"));
                }
            }

            Console.WriteLine("\n" + langVM.GetString("execution_finished"));
        }

        private void MenuDeleteJob()
        {
            Console.WriteLine("=== " + langVM.GetString("delete_title") + " ===");
            Console.WriteLine("(" + langVM.GetString("exit_hint") + ")");

            List<Backup> jobs = saveVM.GetAllJobs();
            if (jobs.Count == 0)
            {
                Console.WriteLine(langVM.GetString("error_no_jobs"));
                return;
            }

            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {jobs[i].Name}");
            }

            Console.Write("\n" + langVM.GetString("prompt_delete") + " ");
            string input = ReadInputOrCancel();

            if (int.TryParse(input, out int index) && index > 0 && index <= jobs.Count)
            {
                string nomASupprimer = jobs[index - 1].Name;
                saveVM.DeleteJob(nomASupprimer);
                Console.WriteLine(langVM.GetString("success_delete").Replace("{nom}", nomASupprimer));
            }
            else
            {
                Console.WriteLine(langVM.GetString("error_invalid_input"));
            }
        }

        private void MenuChangeLanguage()
        {
            try
            {
                Console.WriteLine("=== " + langVM.GetString("lang_title") + " ===");
                Console.WriteLine("(" + langVM.GetString("exit_hint") + ")");
                Console.Write(langVM.GetString("prompt_lang") + " ");

                string lang = ReadInputOrCancel();

                langVM.UpdateLanguage(lang);

                Console.WriteLine(langVM.GetString("success_lang"));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("\n[!] " + langVM.GetString("error_invalid_input") + " (en/fr only)");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n" + langVM.GetString("action_cancelled"));
            }
        }

        // Throws OperationCanceledException if the user types "exit".
        private string ReadInputOrCancel()
        {
            string input = Console.ReadLine();

            if (input?.ToLower() == "exit")
            {
                throw new OperationCanceledException();
            }

            return input;
        }
    }
}