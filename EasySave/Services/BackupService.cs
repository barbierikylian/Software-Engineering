using EasyLog;
using EasySave.Model;
using EasySave.Services;
using System.Text.Json;

namespace EasySave.Service
{
    public class BackupService
    {
        private const int MaxJobs = 5;

        private static readonly string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string ConfigDir = Path.Combine(AppDataFolder, "EasySave", "data");
        private static readonly string JobsFilePath = Path.Combine(ConfigDir, "Listjobs.json");
        private static readonly string StateFilePath = Path.Combine(ConfigDir, "state.json");

        public List<Backup> Jobs { get; private set; } = new();

        public BackupService()
        {
            Directory.CreateDirectory(ConfigDir);
            LoadJobs();
        }

        public void PerformJobs(Backup job)
        {
            ILogStrategy liveLogger = new LogLive(StateFilePath);

            var stats = GetStats(job);

            LogModel liveState = new LogModel
            {
                name = job.Name,
                fileSource = job.FileSource,
                fileDestination = job.FileDestination,
                state = "ACTIVE",
                totalFilesToCopy = stats.count,
                totalFilesSize = stats.size,
                nbFilesLeftToDo = stats.count,
                progression = 0
            };

            liveLogger.WriteLog(liveState);

            ISaveStrategy strategy = job.Type.ToLower() == "differential"
                ? new SaveDifferential()
                : new SaveComplete();

            strategy.Save(job);

            liveState.state = "END";
            liveState.progression = 100;
            liveState.nbFilesLeftToDo = 0;
            liveLogger.WriteLog(liveState);
        }

        private (int count, long size) GetStats(Backup job)
        {
            int count = 0;
            long size = 0;

            if (!Directory.Exists(job.FileSource)) return (0, 0);

            foreach (string file in Directory.GetFiles(job.FileSource, "*.*", SearchOption.AllDirectories))
            {
                string targetFile = Path.Combine(job.FileDestination, Path.GetRelativePath(job.FileSource, file));

                if (job.Type.ToLower() == "full" || !File.Exists(targetFile) || File.GetLastWriteTime(file) > File.GetLastWriteTime(targetFile))
                {
                    count++;
                    size += new FileInfo(file).Length;
                }
            }
            return (count, size);
        }

        public Backup CreateJob(Backup job)
        {
            if (!CanCreateJob()) throw new InvalidOperationException("Max jobs reached.");
            Jobs.Add(job);
            SaveJobs();
            return job;
        }

        public void DeleteJob(Backup job)
        {
            Jobs.Remove(job);
            SaveJobs();
        }

        public List<Backup> GetAllJobs() => new(Jobs);

        public bool CanCreateJob() => Jobs.Count < MaxJobs;

        public void SaveJobs()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(JobsFilePath, JsonSerializer.Serialize(Jobs, options));
        }

        private void LoadJobs()
        {
            if (!File.Exists(JobsFilePath)) return;
            try
            {
                string json = File.ReadAllText(JobsFilePath);
                Jobs = JsonSerializer.Deserialize<List<Backup>>(json) ?? new();
            }
            catch { Jobs = new List<Backup>(); }
        }
    }
}