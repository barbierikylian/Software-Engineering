using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public void SetLogFormat(string format)
        {
            backupService.SetLogFormat(format);
        }

        public void SetLogDestination(string destination)
        {
            backupService.SetLogDestination(destination);
        }

        public void SetServerUrl(string url)
        {
            backupService.SetServerUrl(url);
        }

        public void SetLogUserName(string name)
        {
            backupService.SetLogUserName(name);
        }

        public bool CanCreateNewJob()
        {
            return backupService.CanCreateJob();
        }

        public void CreateJob(string name, string source, string destination, string type)
        {
            Backup job = new Backup();
            job.Name = name;
            job.FileSource = source;
            job.FileDestination = destination;
            job.Type = type;

            backupService.CreateJob(job);
        }

        public void DeleteJob(string jobName)
        {
            List<Backup> jobs = backupService.GetAllJobs();
            Backup jobToDelete = null;

            foreach (Backup j in jobs)
            {
                if (j.Name == jobName)
                {
                    jobToDelete = j;
                    break;
                }
            }

            if (jobToDelete != null)
            {
                backupService.DeleteJob(jobToDelete);
            }
        }

        public List<Backup> GetAllJobs()
        {
            return backupService.GetAllJobs();
        }

        public void PauseJob(string jobName)
        {
            backupService.PauseJob(jobName);
        }

        public void ResumeJob(string jobName)
        {
            backupService.ResumeJob(jobName);
        }

        public void StopJob(string jobName)
        {
            backupService.StopJob(jobName);
        }

        public async Task<string> PerformJobsAsync(string sequence, string businessSoftware, string encryptedExtensions, string priorityExtensions, long maxFileSizeBytes, IProgress<int> progress = null, Action<string> currentFileCallback = null)
        {
            List<Backup> jobs = backupService.GetAllJobs();
            Backup jobToRun = null;

            foreach (Backup j in jobs)
            {
                if (j.Name == sequence)
                {
                    jobToRun = j;
                    break;
                }
            }

            if (jobToRun != null)
            {
                return await backupService.PerformJobsAsync(jobToRun, businessSoftware, encryptedExtensions, priorityExtensions, maxFileSizeBytes, progress, currentFileCallback);
            }
            else if (string.IsNullOrWhiteSpace(sequence))
            {
                List<Task<string>> tasks = new List<Task<string>>();
                foreach (Backup job in jobs)
                {
                    tasks.Add(backupService.PerformJobsAsync(job, businessSoftware, encryptedExtensions, priorityExtensions, maxFileSizeBytes, progress, currentFileCallback));
                }

                string[] results = await Task.WhenAll(tasks);
                List<string> errors = new List<string>();

                for (int i = 0; i < results.Length; i++)
                {
                    if (string.IsNullOrEmpty(results[i]) == false)
                    {
                        errors.Add("[" + jobs[i].Name + "] " + results[i]);
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