namespace EasySave.Model
{
    /// <summary>
    /// Represents the real-time progress state of a running backup job.
    /// Used to populate the state.json status file.
    /// </summary>
    public class ActiveJob
    {
        /// <summary>Full path of the file currently being copied (source).</summary>
        public string NameFile { get; set; } = string.Empty;

        /// <summary>Total size (in bytes) of all files eligible for this backup.</summary>
        public long SizeFile { get; set; }

        /// <summary>Progression percentage (0–100).</summary>
        public int Progression { get; set; }

        /// <summary>Number of files still remaining to copy.</summary>
        public int FileRemaining { get; set; }

        /// <summary>Total size (in bytes) of files still remaining to copy.</summary>
        public long SizeFileRemaining { get; set; }
    }
}