using System;
using System.Collections.Generic;
using EasySave.View;
using EasySave.ViewModel;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "EasySave - Console";
            SaveViewModel saveViewModel = new SaveViewModel();
            LanguageViewModel languageViewModel = new LanguageViewModel();

            if (args.Length > 0)
            {
                string inputUser = string.Join(" ", args); ;
                List<int> jobsToExecute = new List<int>();

                try
                {
                    if (inputUser.Contains("-"))
                    {
                        string[] parts = inputUser.Split('-');
                        int start = int.Parse(parts[0]);
                        int end = int.Parse(parts[1]);
                        for (int i = start; i <= end; i++) jobsToExecute.Add(i);
                    }
                    else if (inputUser.Contains(";"))
                    {
                        string[] parts = inputUser.Split(';');
                        foreach (var part in parts)
                        {
                            jobsToExecute.Add(int.Parse(part));
                        }
                    }
                    else
                    {
                        jobsToExecute.Add(int.Parse(inputUser));
                    }

                    foreach (int jobNum in jobsToExecute)
                    {
                        var allJobsList = saveViewModel.GetAllJobs();
                        int index = jobNum - 1;

                        if (index >= 0 && index < allJobsList.Count)
                        {
                            string nomReel = allJobsList[index].Name;

                            Console.WriteLine($"\n[CLI] Lancement du travail : {nomReel}...");

                            saveViewModel.PerformJobs(nomReel);
                        }
                        else
                        {
                            Console.WriteLine($"\n[Erreur] Aucun travail trouvé pour le n°{jobNum}.");
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Erreur : Format d'argument invalide. Utilisez '1-3' ou '1;3'.");
                }
            }
            else
            {
                ConsoleView view = new ConsoleView(saveViewModel, languageViewModel);
                view.Display();
            }
        }
    }
}