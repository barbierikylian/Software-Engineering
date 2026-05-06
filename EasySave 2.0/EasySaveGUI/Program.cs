using System;
using System.Collections.Generic;
using System.Windows;
using EasySave.Model;
using EasySave.ViewModel;

namespace EasySaveGUI
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            SaveViewModel saveViewModel = new SaveViewModel();

            if (args.Length > 0)
            {
                string inputUser = string.Join("", args).Trim();
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
                        foreach (string part in parts)
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
                        List<Backup> allJobsList = saveViewModel.GetAllJobs();
                        int index = jobNum - 1;

                        if (index >= 0 && index < allJobsList.Count)
                        {
                            string targetName = allJobsList[index].Name;
                            saveViewModel.PerformJobs(targetName, "");
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            else
            {
                App app = new App();

                MainWindow window = new MainWindow();
                app.Run(window);
            }
        }
    }
}