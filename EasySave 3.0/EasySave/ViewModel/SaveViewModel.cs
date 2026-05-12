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

        public void PauseJob(string jobName) => backupService.PauseJob(jobName);
        public void ResumeJob(string jobName) => backupService.ResumeJob(jobName);
        public void StopJob(string jobName) => backupService.StopJob(jobName);

        public async Task<string> PerformJobsAsync(string sequence, string businessSoftware, string encryptedExtensions, string priorityExtensions, IProgress<int> progress = null, Action<string> currentFileCallback = null)
        {
            List<Backup> jobs = backupService.GetAllJobs();
            Backup jobToRun = jobs.Find(j => j.Name == sequence);

            if (jobToRun != null)
            {
                return await backupService.PerformJobsAsync(jobToRun, businessSoftware, encryptedExtensions, priorityExtensions, progress, currentFileCallback);
            }
            else if (string.IsNullOrWhiteSpace(sequence))
            {
                var tasks = jobs.Select(job => backupService.PerformJobsAsync(job, businessSoftware, encryptedExtensions, priorityExtensions, progress, currentFileCallback)).ToList();
                string[] results = await Task.WhenAll(tasks);

                List<string> errors = new List<string>();
                for (int i = 0; i < results.Length; i++)
                {
                    if (!string.IsNullOrEmpty(results[i]))
                    {
                        errors.Add($"[{jobs[i].Name}] {results[i]}");
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