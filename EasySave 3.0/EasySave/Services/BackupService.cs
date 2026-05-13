using EasyLog;
using EasySave.Model;
using EasySave.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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

        // --- AJOUTS POUR LE RÉSEAU ---
        private string _logDestination = "Both";
        private string _serverUrl = "http://localhost:8080/api/logs";

        public void SetLogDestination(string destination) => _logDestination = destination;
        public void SetServerUrl(string url) => _serverUrl = url;
        // -----------------------------

        public ConcurrentDictionary<string, CancellationTokenSource> CancelTokens { get; } = new();
        public ConcurrentDictionary<string, ManualResetEventSlim> PauseEvents { get; } = new();

        public BackupService()
        {
            Directory.CreateDirectory(ConfigDir);
            LoadJobs();
        }

        public void SetLogFormat(string format) => _currentLogFormat = format.ToLower();

        public async Task<string> PerformJobsAsync(Backup job, string businessSoftware, string encryptedExtensions, string priorityExtensions, long maxFileSizeBytes, IProgress<int> progress = null, Action<string> currentFileCallback = null)
        {
            if (!PauseEvents.ContainsKey(job.Name))
                PauseEvents[job.Name] = new ManualResetEventSlim(true);
            if (!CancelTokens.ContainsKey(job.Name))
                CancelTokens[job.Name] = new CancellationTokenSource();

            var pauseEvent = PauseEvents[job.Name];
            var cts = CancelTokens[job.Name];

            IFormatter formatter = _currentLogFormat == "xml" ? new XmlFormatter() : new JsonFormatter();
            ILogStrategy logger = new LogLive(StateFilePath, formatter);

            try
            {
                ISaveStrategy strategy = job.Type.ToLower() == "differential" ? new SaveDifferential() : new SaveComplete();
                // --- ON PASSE LES VARIABLES ICI ---
                return await strategy.SaveAsync(job, businessSoftware, encryptedExtensions, priorityExtensions, maxFileSizeBytes, _logDestination, _serverUrl, logger, formatter, progress, currentFileCallback, cts.Token, pauseEvent);
            }
            catch (OperationCanceledException)
            {
                return "Job stopped.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
            finally
            {
                PauseEvents.TryRemove(job.Name, out _);
                CancelTokens.TryRemove(job.Name, out _);
            }
        }

        public void PauseJob(string jobName)
        {
            if (PauseEvents.TryGetValue(jobName, out var pauseEvent))
                pauseEvent.Reset();
        }

        public void ResumeJob(string jobName)
        {
            if (PauseEvents.TryGetValue(jobName, out var pauseEvent))
                pauseEvent.Set();
        }

        public void StopJob(string jobName)
        {
            if (CancelTokens.TryGetValue(jobName, out var cts))
                cts.Cancel();
        }

        public void CreateJob(Backup job)
        {
            Jobs.Add(job);
            SaveJobs();
        }

        public void DeleteJob(Backup job)
        {
            Backup item = Jobs.Find(j => j.Name == job.Name);
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