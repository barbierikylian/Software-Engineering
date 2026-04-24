namespace EasySave.Model
{
    /// <summary>
    /// Represents a backup job configuration and its current status.
    /// Serialized to / deserialized from the JSON config and state files.
    /// </summary>
    public class Backup
    {
        /// <summary>Display name of the backup job.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Timestamp of the last action performed by this job.</summary>
        ///public DateTime Timestamp { get; set; }

        /// <summary>
        /// Current status of the job (e.g. "Active", "Inactive", "Error").
        /// Also carries real-time progress data when the job is running.
        /// </summary>
        ///public ActiveJob BackupStatus { get; set; } = new ActiveJob();

        /// <summary>Full path of the source directory (local, external or UNC network path).</summary>
        public string FileSource { get; set; } = string.Empty;

        /// <summary>Full path of the destination directory (local, external or UNC network path).</summary>
        public string DestinationFile { get; set; } = string.Empty;

        /// <summary>
        /// Type of backup: "Full" or "Differential".
        /// Determines which ISaveStrategy implementation is used at runtime.
        /// </summary>
        public string Type { get; set; } = string.Empty;
    }
}