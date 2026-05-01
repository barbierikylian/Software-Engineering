using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyLog
{
    // Log strategy that OVERWRITES a single file (state.json) with the current job state.
    public class LogLive : ILogStrategy
    {
        private readonly string _filePath;

        public LogLive(string filePath)
        {
            _filePath = filePath;

            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void WriteLog(LogModel logModel)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            string text = JsonSerializer.Serialize(logModel, options);
            File.WriteAllText(_filePath, text);
        }
    }
}