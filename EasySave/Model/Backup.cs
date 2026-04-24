namespace EasySave.Model
{
    public class Backup
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public ActiveJob BackupStatus { get; set; } = new ActiveJob();
        public string FileSource { get; set; } = string.Empty;
        public string FileDestination { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}