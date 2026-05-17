using EasySave.Model;
using EasySave.Service;
using EasySave.View;
using EasySave;
using System;
using System.Collections.Generic;
using System.Windows;

namespace EasySave
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                BackupService backupService = new BackupService();
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

                    long maxFileSizeBytes = 50 * 1024 * 1024;
                    List<Backup> allJobsList = backupService.GetAllJobs();

                    foreach (int jobNum in jobsToExecute)
                    {
                        int index = jobNum - 1;

                        if (index >= 0 && index < allJobsList.Count)
                        {
                            Backup targetJob = allJobsList[index];
                            backupService.PerformJobsAsync(targetJob, "", "", "", maxFileSizeBytes).Wait();
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