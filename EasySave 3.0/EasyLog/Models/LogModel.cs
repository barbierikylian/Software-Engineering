using System;
using System.Text.Json.Serialization;

namespace EasyLog
{
    // Data model for a log entry, shared by LogDaily and LogLive.
    public class LogModel
    {
        // For Both
        public string? name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? fileSource { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? fileDestination { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? executionTime { get; set; }

        // For Daily
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? time { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? fileSize { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? fileTransferTime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? encryptionTime { get; set; }

        // For Live
        public string? state { get; set; }

        public int? totalFilesToCopy { get; set; }

        public long? totalFilesSize { get; set; }

        public int? nbFilesLeftToDo { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? sizeFileRemaining { get; set; }

        public int? progression { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? currentSourceFile { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? currentDestinationFile { get; set; }
    }
}