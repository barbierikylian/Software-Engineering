using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyLog
{
    // Log strategy that APPENDS entries to a daily JSON file (yyyy-MM-dd.json).
    public class LogDaily : ILogStrategy
    {
        private readonly string _logDirectory;

        public LogDaily(string path)
        {
            _logDirectory = path;
            Directory.CreateDirectory(_logDirectory);
        }

        public void WriteLog(LogModel logModel)
        {
            string name = DateTime.Now.ToString("yyyy-MM-dd") + ".json";
            string path = Path.Combine(_logDirectory, name);

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            string text = JsonSerializer.Serialize(logModel, options);

            File.AppendAllText(path, text + "\n");
        }
    }
}