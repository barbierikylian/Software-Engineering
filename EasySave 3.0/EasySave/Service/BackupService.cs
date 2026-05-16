using EasyLog;
using EasySave.Model;
using EasySave.Service;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

        public List<Backup> Jobs { get; private set; } = new List<Backup>();
        private string _currentLogFormat = "json";

        private string _logDestination = "Both";
        private string _serverUrl = "";
        private string _logUserName = Environment.UserName;

        public ConcurrentDictionary<string, CancellationTokenSource> CancelTokens { get; } = new ConcurrentDictionary<string, CancellationTokenSource>();
        public ConcurrentDictionary<string, ManualResetEventSlim> PauseEvents { get; } = new ConcurrentDictionary<string, ManualResetEventSlim>();

        public BackupService()
        {
            Directory.CreateDirectory(ConfigDir);

            try
            {
                string directoryPath = Path.GetDirectoryName(StateFilePath);
                if (Directory.Exists(directoryPath))
                {
                    if (File.Exists(StateFilePath + ".json"))
                    {
                        File.WriteAllText(StateFilePath + ".json", "[]");
                    }

                    if (File.Exists(StateFilePath + ".xml"))
                    {
                        File.WriteAllText(StateFilePath + ".xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Logs></Logs>");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Content clear failed: " + ex.Message);
            }

            LoadJobs();
        }

        public void SetLogDestination(string destination)
        {
            _logDestination = destination;
        }

        public void SetServerUrl(string url)
        {
            _serverUrl = url;
        }

        public void SetLogUserName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logUserName = Environment.UserName;
            }
            else
            {
                _logUserName = name;
            }
        }

        public void SetLogFormat(string format)
        {
            _currentLogFormat = format.ToLower();
        }

        public async Task<string> PerformJobsAsync(Backup job, string businessSoftware, string encryptedExtensions, string priorityExtensions, long maxFileSizeBytes, IProgress<int> progress = null, Action<string> currentFileCallback = null)
        {
            if (PauseEvents.ContainsKey(job.Name) == false)
            {
                PauseEvents[job.Name] = new ManualResetEventSlim(true);
            }

            if (CancelTokens.ContainsKey(job.Name) == false)
            {
                CancelTokens[job.Name] = new CancellationTokenSource();
            }

            ManualResetEventSlim pauseEvent = PauseEvents[job.Name];
            CancellationTokenSource cts = CancelTokens[job.Name];

            IFormatter formatter;
            if (_currentLogFormat == "xml")
            {
                formatter = new XmlFormatter();
            }
            else
            {
                formatter = new JsonFormatter();
            }

            ILogStrategy logger = new LogLive(StateFilePath, formatter);

            try
            {
                ISaveStrategy strategy;
                if (job.Type.ToLower() == "differential")
                {
                    strategy = new SaveDifferential();
                }
                else
                {
                    strategy = new SaveComplete();
                }

                return await strategy.SaveAsync(job, businessSoftware, encryptedExtensions, priorityExtensions, maxFileSizeBytes, _logDestination, _serverUrl, _logUserName, logger, formatter, progress, currentFileCallback, cts.Token, pauseEvent);
            }
            catch (OperationCanceledException)
            {
                return "Job stopped.";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
            finally
            {
                ManualResetEventSlim removedPauseEvent;
                PauseEvents.TryRemove(job.Name, out removedPauseEvent);

                CancellationTokenSource removedCts;
                CancelTokens.TryRemove(job.Name, out removedCts);
            }
        }

        public void PauseJob(string jobName)
        {
            ManualResetEventSlim pauseEvent;
            if (PauseEvents.TryGetValue(jobName, out pauseEvent))
            {
                pauseEvent.Reset();
            }
        }

        public void ResumeJob(string jobName)
        {
            ManualResetEventSlim pauseEvent;
            if (PauseEvents.TryGetValue(jobName, out pauseEvent))
            {
                pauseEvent.Set();
            }
        }

        public void StopJob(string jobName)
        {
            CancellationTokenSource cts;
            if (CancelTokens.TryGetValue(jobName, out cts))
            {
                cts.Cancel();
            }
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

        public List<Backup> GetAllJobs()
        {
            List<Backup> copyList = new List<Backup>(Jobs);
            return copyList;
        }

        public bool CanCreateJob()
        {
            return true;
        }

        public void SaveJobs()
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                string jsonString = JsonSerializer.Serialize(Jobs, options);
                File.WriteAllText(JobsFilePath, jsonString);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to write jobs to disk: " + ex.Message);
            }
        }

        private void LoadJobs()
        {
            if (File.Exists(JobsFilePath) == false)
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(JobsFilePath);
                List<Backup> loadedJobs = JsonSerializer.Deserialize<List<Backup>>(json);

                if (loadedJobs != null)
                {
                    Jobs = loadedJobs;
                }
                else
                {
                    Jobs = new List<Backup>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading jobs: " + ex.Message);
                Jobs = new List<Backup>();
            }
        }

        public void RemoveFromStateLog(string jobName)
        {
            IFormatter formatter;
            if (_currentLogFormat == "xml")
            {
                formatter = new XmlFormatter();
            }
            else
            {
                formatter = new JsonFormatter();
            }

            ILogStrategy logger = new LogLive(StateFilePath, formatter);
            logger.RemoveLog(jobName);
        }
    }
}