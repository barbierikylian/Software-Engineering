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
                PrintHeader();

                PrintMenuOption("1", langVM.GetString("menu_create"), ConsoleColor.Green);
                PrintMenuOption("2", langVM.GetString("menu_execute"), ConsoleColor.Green);
                PrintMenuOption("3", langVM.GetString("menu_list"), ConsoleColor.Green);
                PrintMenuOption("4", langVM.GetString("menu_delete"), ConsoleColor.Green);
                PrintMenuOption("5", langVM.GetString("menu_language"), ConsoleColor.Yellow);
                PrintMenuOption("6", langVM.GetString("menu_exit"), ConsoleColor.Red);

                Console.WriteLine("\n" + new string('─', 40));
                Console.Write($" {langVM.GetString("choose_option")} ");

                string choice = Console.ReadLine();

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
                            Console.WriteLine("\n " + langVM.GetString("goodbye"));
                            isRunning = false;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n [!] " + langVM.GetString("error_invalid_option"));
                            Console.ResetColor();
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n > " + langVM.GetString("action_cancelled"));
                    Console.ResetColor();
                }

                if (isRunning)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\n " + langVM.GetString("press_enter"));
                    Console.ResetColor();
                    Console.ReadLine();
                }
            }
        }

        private void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"  ______                         _____                ");
            Console.WriteLine(@" |  ____|                       / ____|               ");
            Console.WriteLine(@" | |__   __ _ ___ _   _   _____| (___   __ ___   _____ ");
            Console.WriteLine(@" |  __| / _` / __| | | | / ___| \___ \ / _` \ \ / / _ \");
            Console.WriteLine(@" | |___| (_| \__ \ |_| | \___ \ ____) | (_| |\ V /  __/");
            Console.WriteLine(@" |______\__,_|___/\__, | /____/|_____/ \__,_| \_/ \___|");
            Console.WriteLine(@"                   |___/                              ");
            Console.WriteLine("\n " + langVM.GetString("welcome").ToUpper());
            Console.WriteLine(new string('═', 55) + "\n");
            Console.ResetColor();
        }

        private void PrintMenuOption(string key, string label, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write($"  [{key}] ");
            Console.ResetColor();
            Console.WriteLine(label);
        }

        private void MenuCreateJob()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"--- {langVM.GetString("create_title")} ---");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($" (Type 'exit' to cancel and return to menu)\n");
            Console.ResetColor();

            if (!saveVM.CanCreateNewJob())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(langVM.GetString("error_max_jobs"));
                Console.ResetColor();
                return;
            }

            Console.Write($" {langVM.GetString("label_name")} : ");
            string name = ReadInputOrCancel();

            Console.Write($" {langVM.GetString("label_source")} : ");
            string source = ReadInputOrCancel();

            Console.Write($" {langVM.GetString("label_dest")} : ");
            string destination = ReadInputOrCancel();

            Console.Write($" {langVM.GetString("label_type")} : ");
            string type = ReadInputOrCancel();

            saveVM.CreateJob(name, source, destination, type);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n [OK] " + langVM.GetString("success_create").Replace("{name}", name));
            Console.ResetColor();
        }

        private void MenuListJobs()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"--- {langVM.GetString("list_title")} ---\n");
            Console.ResetColor();

            List<Backup> jobs = saveVM.GetAllJobs();
            if (jobs.Count == 0)
            {
                Console.WriteLine(" " + langVM.GetString("error_no_jobs"));
                return;
            }

            foreach (Backup job in jobs)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" • ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{job.Name.PadRight(15)} ");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"[{job.Type}]");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"   {job.FileSource} -> {job.FileDestination}\n");
            }
            Console.ResetColor();
        }

        private void MenuExecuteJob()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"--- {langVM.GetString("execute_title")} ---\n");
            Console.ResetColor();

            List<Backup> jobs = saveVM.GetAllJobs();
            if (jobs.Count == 0) { Console.WriteLine(" " + langVM.GetString("error_no_jobs")); return; }

            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"  [{i + 1}] {jobs[i].Name}");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  (!) {langVM.GetString("hint_multiple")} (ex: 1;3)");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  (Type 'exit' to cancel)");
            Console.ResetColor();

            Console.Write($"\n {langVM.GetString("prompt_execute")} ");
            string input = ReadInputOrCancel();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("\n >>> " + langVM.GetString("executing_all"));
                saveVM.PerformJobs("");
            }
            else
            {
                string[] parts = input.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> selectedJobs = new List<string>();

                foreach (string part in parts)
                {
                    int index;
                    if (int.TryParse(part, out index) && index > 0 && index <= jobs.Count)
                    {
                        selectedJobs.Add(jobs[index - 1].Name);
                    }
                }

                if (selectedJobs.Count > 0)
                {
                    foreach (string jobName in selectedJobs)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\n " + langVM.GetString("executing_single").Replace("{name}", jobName));
                        Console.ResetColor();
                        saveVM.PerformJobs(jobName);
                    }
                }
            }
        }

        private void MenuDeleteJob()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"--- {langVM.GetString("delete_title")} ---\n");
            Console.ResetColor();

            List<Backup> jobs = saveVM.GetAllJobs();
            if (jobs.Count == 0) return;

            for (int i = 0; i < jobs.Count; i++) { Console.WriteLine($"  [{i + 1}] {jobs[i].Name}"); }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n (Type 'exit' to cancel)");
            Console.ResetColor();

            Console.Write($"\n {langVM.GetString("prompt_delete")} ");
            string input = ReadInputOrCancel();

            int index;
            if (int.TryParse(input, out index) && index > 0 && index <= jobs.Count)
            {
                string nameToDelete = jobs[index - 1].Name;
                saveVM.DeleteJob(nameToDelete);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n [OK] " + langVM.GetString("success_delete").Replace("{name}", nameToDelete));
                Console.ResetColor();
            }
        }

        private void MenuChangeLanguage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"--- {langVM.GetString("lang_title")} ---\n");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($" (Type 'exit' to cancel)");
            Console.ResetColor();
            Console.Write($"\n {langVM.GetString("prompt_lang")} ");

            try
            {
                string lang = ReadInputOrCancel();
                langVM.UpdateLanguage(lang);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n [OK] " + langVM.GetString("success_lang"));
            }
            catch { Console.WriteLine("\n [!] Error (en/fr only)"); }
            Console.ResetColor();
        }

        private string ReadInputOrCancel()
        {
            string input = Console.ReadLine();
            if (input?.ToLower() == "exit") throw new OperationCanceledException();
            return input;
        }
    }
}