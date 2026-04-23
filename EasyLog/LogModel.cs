namespace EasyLog
{
    public class LogModel
    {
        public DateTime Timestamp { get; set; }
        public string? BackupName { get; set; }
        public string? SourceFile { get; set; }
        public string? DestinationFile { get; set; }
        public long FileSize { get; set; }
        public long TransferTime { get; set; }  
    }
}
