using System;
using System.Collections.Generic;
using EasySave.Model;
using EasySave.Service;

namespace EasySave.ViewModel
{
    public class SaveViewModel
    {
        private BackupService backupservice;

        public SaveViewModel()
        {
            backupservice = new BackupService();
        }

        public bool CanCreateNewJob()
        {
            return backupservice.CanCreateJob();
        }

        public void CreateJob(string name, string source, string destination, string type)
        {
            Backup job = new Backup();
            job.Name = name;
            job.FileSource = source;
            job.FileDestination = destination;
            job.Type = type;

            backupservice.CreateJob(job);
        }

        public void DeleteJob(string jobName)
        {
            Backup jobToDelete = backupservice.GetAllJobs().Find(j => j.Name == jobName);

            if (jobToDelete != null)
            {
                backupservice.DeleteJob(jobToDelete);
            }
        }

        public List<Backup> GetAllJobs()
        {
            return backupservice.GetAllJobs();
        }

        public void PerformJobs(string sequence)
        {
            List<Backup> jobs = backupservice.GetAllJobs();

            if (jobs.Count == 0) return;

            Backup jobToRun = jobs.Find(j => j.Name == sequence);

            if (jobToRun != null)
            {
                backupservice.PerformJobs(jobToRun);
            }
            else if (string.IsNullOrWhiteSpace(sequence))
            {
                foreach (Backup job in jobs)
                {
                    backupservice.PerformJobs(job);
                }
            }
        }
    }
}