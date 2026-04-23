using System;
using System.Text.Json;

namespace EasyLog
{
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

            string text = JsonSerializer.Serialize(logModel, options);

            File.AppendAllText(path, text + "\n");
        }
    }
}
