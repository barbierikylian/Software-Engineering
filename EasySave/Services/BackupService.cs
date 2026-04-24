using System.Text.Json;
using EasySave.Model;
using EasySave.Services;

namespace EasySave.Service
{
    public class BackupService
    {
        private const int MaxJobs = 5;

        private static readonly string ConfigDir =
            Path.Combine(AppContext.BaseDirectory, "data");

        private static readonly string JobsFilePath =
            Path.Combine(ConfigDir, "jobs.json");

        private static readonly string StateFilePath =
            Path.Combine(ConfigDir, "state.json");

        public List<Backup> Jobs { get; private set; } = new();

        public BackupService()
        {
            Directory.CreateDirectory(ConfigDir);
            LoadJobs();
        }
        public Backup CreateJob(Backup job)
        {
            if (!CanCreateJob())
                throw new InvalidOperationException(
                    $"Cannot create more than {MaxJobs} backup jobs.");

            Jobs.Add(job);
            SaveJobs();
            return job;
        }
        public void DeleteJob(Backup job)
        {
            Jobs.Remove(job);
            SaveJobs();
        }

        public List<Backup> UpdateList(Backup updatedJob)
        {
            int index = Jobs.FindIndex(j => j.Name == updatedJob.Name);
            if (index >= 0)
                Jobs[index] = updatedJob;

            SaveJobs();
            return Jobs;
        }

        public List<Backup> GetAllJobs() => new(Jobs);
        public void PerformJobs(Backup job)
        {
            job.Timestamp = DateTime.Now;
            job.BackupStatus.Progression = 0;
            UpdateStateFile();

            ISaveStrategy strategy = job.Type.ToLowerInvariant() == "differential"
                ? new SaveDifferential()
                : new SaveComplete();

            strategy.Save(job);

            job.BackupStatus.Progression = 100;
            job.BackupStatus.FileRemaining = 0;
            UpdateStateFile();
        }

        public void SaveJobs()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Jobs, options);
            File.WriteAllText(JobsFilePath, json);
        }

        public bool CanCreateJob() => Jobs.Count < MaxJobs;

        private void LoadJobs()
        {
            if (!File.Exists(JobsFilePath))
                return;

            try
            {
                string json = File.ReadAllText(JobsFilePath);
                Jobs = JsonSerializer.Deserialize<List<Backup>>(json) ?? new();
            }
            catch
            {
                Jobs = new List<Backup>();
            }
        }

        private void UpdateStateFile()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Jobs, options);
            File.WriteAllText(StateFilePath, json);
        }
    }
}