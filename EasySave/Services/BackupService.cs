using EasyLog;
using EasySave.Model;
using EasySave.Services;
using System.Text.Json;

namespace EasySave.Service
{
    public class BackupService
    {
        private const int MaxJobs = 5;

        private static readonly string AppDataFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private static readonly string ConfigDir =
            Path.Combine(AppDataFolder, "EasySave", "data");

        private static readonly string JobsFilePath =
            Path.Combine(ConfigDir, "Listjobs.json");

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
            string stateFilePath = Path.Combine(AppContext.BaseDirectory, "logs", "state.json");
            ILogStrategy liveLogger = new LogLive(stateFilePath);

            LogModel liveState = new LogModel
            {
                name = job.Name,
                fileSource = job.FileSource,
                fileDestination = job.FileDestination,
                state = "ACTIVE",
                progression = 0,
                totalFilesToCopy = 0,
                totalFilesSize = 0,
                nbFilesLeftToDo = 0
            };

            liveLogger.WriteLog(liveState);

            ISaveStrategy strategy = job.Type.ToLowerInvariant() == "differential"
                ? new SaveDifferential()
                : new SaveComplete();

            strategy.Save(job);

            liveState.state = "END";
            liveState.progression = 100;
            liveState.nbFilesLeftToDo = 0;

            liveLogger.WriteLog(liveState);
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