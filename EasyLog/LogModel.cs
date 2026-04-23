using System;

namespace EasyLog
{
    public class LogModel
    {
        // For Both
        public string? Name { get; set; }
        public string? FileSource { get; set; }
        public string? FileDestination { get; set; }

        // For Daily
        public DateTime Time { get; set; }
        public long FileSize { get; set; }
        public double FileTransferTime { get; set; }


        // For Live
        public string State { get; set; }
        public int? TotalFilesToCopy { get; set; }
        public long? TotalFilesSize { get; set; }
        public int? NbFilesLeftToDo { get; set; }
        public int? Progression { get; set; }
    }
}
