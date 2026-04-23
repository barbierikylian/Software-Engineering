namespace EasyLog
{
    public class LogModel
    {
        public DateTime Time { get; set; }
        public string? Name { get; set; }
        public string? FileSource { get; set; }
        public string? FileDestination { get; set; }
        public long FileSize { get; set; }
        public long FileTransferTime { get; set; }
    }
}
