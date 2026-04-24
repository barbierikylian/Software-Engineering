using System;

namespace EasyLog
{
    public class LogModel
    {
        // For Both
        public string? name { get; set; }
        public string? fileSource { get; set; }
        public string? fileDestination { get; set; }

        // For Daily
        public DateTime time { get; set; }
        public long fileSize { get; set; }
        public double fileTransferTime { get; set; }


        // For Live
        public string state { get; set; }
        public int? totalFilesToCopy { get; set; }
        public long? totalFilesSize { get; set; }
        public int? nbFilesLeftToDo { get; set; }
        public int? progression { get; set; }
    }
}
