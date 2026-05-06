using EasyLog;
using EasySave.Model;
using EasySave.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace EasySave.Service
{
    public class BackupService
    {
        private static readonly string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string ConfigDir = Path.Combine(AppDataFolder, "EasySave", "data");
        private static readonly string JobsFilePath = Path.Combine(ConfigDir, "Listjobs.json");
        private static readonly string StateFilePath = Path.Combine(ConfigDir, "state");

        public List<Backup> Jobs { get; private set; } = new();
        private string _currentLogFormat = "json";

        public BackupService()
        {
            Directory.CreateDirectory(ConfigDir);
            LoadJobs();
        }

        public void SetLogFormat(string format) => _currentLogFormat = format.ToLower();

        public string PerformJobs(Backup job, string businessSoftware, IProgress<int> progress = null, Action<string> currentFileCallback = null)
        {
            if (!Directory.Exists(job.FileSource))
                return $"Source directory not found: {job.FileSource}";

            if (!Directory.Exists(job.FileDestination))
                return $"Destination directory not found: {job.FileDestination}";

            IFormatter formatter = _currentLogFormat == "xml" ? new XmlFormatter() : new JsonFormatter();

            ILogStrategy liveLogger = new LogLive(StateFilePath, formatter);

            ISaveStrategy strategy = job.Type.ToLower() == "differential"
                ? new SaveDifferential()
                : new SaveComplete();

            return strategy.Save(job, businessSoftware, liveLogger, formatter, progress, currentFileCallback);
        }

        public void CreateJob(Backup job)
        {
            Jobs.Add(job);
            SaveJobs();
        }

        public void DeleteJob(Backup job)
        {
            var item = Jobs.Find(j => j.Name == job.Name);
            if (item != null)
            {
                Jobs.Remove(item);
                SaveJobs();
            }
        }

        public List<Backup> GetAllJobs() => new(Jobs);

        public bool CanCreateJob() => true;

        public void SaveJobs()
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(JobsFilePath, JsonSerializer.Serialize(Jobs, options));
            }
            catch { }
        }

        private void LoadJobs()
        {
            if (!File.Exists(JobsFilePath)) return;
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
    }
}