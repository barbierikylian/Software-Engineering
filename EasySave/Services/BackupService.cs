using System.Text.Json;
using EasySave.Model;

namespace EasySave.Service
{
    /// <summary>
    /// Core service that manages the lifecycle of backup jobs (create, delete,
    /// persist, execute). Maximum of 5 concurrent jobs is enforced.
    /// </summary>
    public class BackupService
    {
        // ------------------------------------------------------------------ //
        //  Constants                                                           //
        // ------------------------------------------------------------------ //

        private const int MaxJobs = 5;

        // ------------------------------------------------------------------ //
        //  Configuration & state file paths                                   //
        //  Stored next to the executable so they work on any customer server. //
        // ------------------------------------------------------------------ //

        private static readonly string ConfigDir =
            Path.Combine(AppContext.BaseDirectory, "data");

        private static readonly string JobsFilePath =
            Path.Combine(ConfigDir, "jobs.json");

        private static readonly string StateFilePath =
            Path.Combine(ConfigDir, "state.json");

        // ------------------------------------------------------------------ //
        //  Fields & properties                                                 //
        // ------------------------------------------------------------------ //

        /// <summary>In-memory list of all backup jobs (max 5).</summary>
        public List<Backup> Jobs { get; private set; } = new();

        // ------------------------------------------------------------------ //
        //  Constructor                                                         //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Initialises the service: ensures the data directory exists and
        /// loads any previously persisted jobs from disk.
        /// </summary>
        public BackupService()
        {
            Directory.CreateDirectory(ConfigDir);
            LoadJobs();
        }

        // ------------------------------------------------------------------ //
        //  Public methods                                                      //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Creates a new backup job and adds it to the list.
        /// Throws <see cref="InvalidOperationException"/> when the 5-job limit
        /// has been reached.
        /// </summary>
        /// <param name="job">The job to add.</param>
        /// <returns>The added <see cref="Backup"/> instance.</returns>
        public Backup CreateJob(Backup job)
        {
            if (!CanCreateJob())
                throw new InvalidOperationException(
                    $"Cannot create more than {MaxJobs} backup jobs.");

            Jobs.Add(job);
            SaveJobs();
            return job;
        }

        /// <summary>Removes a job from the list and persists the change.</summary>
        /// <param name="job">The job to remove.</param>
        public void DeleteJob(Backup job)
        {
            Jobs.Remove(job);
            SaveJobs();
        }

        /// <summary>
        /// Replaces the current list with an updated one and persists it.
        /// Useful when the ViewModel has already mutated its own copy of the list.
        /// </summary>
        /// <param name="updatedJob">
        /// The modified job (matched by name); its values will be applied in place.
        /// </param>
        /// <returns>The updated list.</returns>
        public List<Backup> UpdateList(Backup updatedJob)
        {
            int index = Jobs.FindIndex(j => j.Name == updatedJob.Name);
            if (index >= 0)
                Jobs[index] = updatedJob;

            SaveJobs();
            return Jobs;
        }

        /// <summary>Returns a copy of all registered backup jobs.</summary>
        public List<Backup> GetAllJobs() => new(Jobs);

        /// <summary>
        /// Executes a single backup job using the strategy that matches its
        /// <see cref="Backup.Type"/> ("Full" → <c>SaveComplete</c>,
        /// "Differential" → <c>SaveDifferential</c>).
        /// </summary>
        /// <param name="job">The job to execute.</param>
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

        /// <summary>Persists the job list to the JSON configuration file.</summary>
        public void SaveJobs()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Jobs, options);
            File.WriteAllText(JobsFilePath, json);
        }

        /// <summary>
        /// Returns <c>true</c> when a new job can still be created
        /// (i.e. fewer than <see cref="MaxJobs"/> jobs currently registered).
        /// </summary>
        public bool CanCreateJob() => Jobs.Count < MaxJobs;

        // ------------------------------------------------------------------ //
        //  Private helpers                                                     //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Loads the job list from disk on startup.
        /// Silently resets to an empty list if the file is absent or corrupt.
        /// </summary>
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

        /// <summary>
        /// Writes the current real-time status of every job to <c>state.json</c>.
        /// Called after every meaningful state change so the file stays in sync.
        /// </summary>
        private void UpdateStateFile()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Jobs, options);
            File.WriteAllText(StateFilePath, json);
        }
    }
}