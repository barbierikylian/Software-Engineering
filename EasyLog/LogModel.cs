using System;

namespace EasyLog
{
    // Data model for a log entry, shared by LogDaily and LogLive.
    public class LogModel
    {
        public string? name { get; set; }
        public string? fileSource { get; set; }
        public string? fileDestination { get; set; }
        public long? executionTime { get; set; }

        public DateTime? time { get; set; }
        public long? fileSize { get; set; }
        public double? fileTransferTime { get; set; }

        public string? state { get; set; }
        public int? totalFilesToCopy { get; set; }
        public long? totalFilesSize { get; set; }
        public int? nbFilesLeftToDo { get; set; }
        public int? progression { get; set; }
    }
}