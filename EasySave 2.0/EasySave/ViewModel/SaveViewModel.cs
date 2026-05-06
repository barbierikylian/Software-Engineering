using System;
using System.Collections.Generic;
using EasySave.Model;
using EasySave.Service;
using EasySave.Services;

namespace EasySave.ViewModel
{
    public class SaveViewModel
    {
        private BackupService backupService;

        public SaveViewModel()
        {
            backupService = new BackupService();
        }

        public void SetLogFormat(string format) => backupService.SetLogFormat(format);

        public bool CanCreateNewJob() => backupService.CanCreateJob();

        public void CreateJob(string name, string source, string destination, string type)
        {
            Backup job = new Backup { Name = name, FileSource = source, FileDestination = destination, Type = type };
            backupService.CreateJob(job);
        }

        public void DeleteJob(string jobName)
        {
            Backup jobToDelete = backupService.GetAllJobs().Find(j => j.Name == jobName);
            if (jobToDelete != null) backupService.DeleteJob(jobToDelete);
        }

        public List<Backup> GetAllJobs() => backupService.GetAllJobs();

        public string PerformJobs(string sequence, string businessSoftware, string encryptedExtensions, IProgress<int> progress = null, Action<string> currentFileCallback = null)
        {
            if (BusinessSoftwareDetector.IsRunning(businessSoftware))
            {
                return $"Error: Business software ({businessSoftware}) is currently running. Backup blocked.";
            }

            List<Backup> jobs = backupService.GetAllJobs();
            Backup jobToRun = jobs.Find(j => j.Name == sequence);

            if (jobToRun != null)
            {
                return backupService.PerformJobs(jobToRun, businessSoftware, encryptedExtensions, progress, currentFileCallback);
            }
            else if (string.IsNullOrWhiteSpace(sequence))
            {
                List<string> errors = new List<string>();

                foreach (Backup job in jobs)
                {
                    string error = backupService.PerformJobs(job, businessSoftware, encryptedExtensions, progress, currentFileCallback);

                    if (!string.IsNullOrEmpty(error))
                    {
                        errors.Add($"[{job.Name}] {error}");
                    }
                }

                if (errors.Count > 0)
                {
                    return string.Join("\n", errors);
                }
            }

            return null;
        }
    }
}