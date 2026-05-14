using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EasyLog
{
    public class LogDaily : ILogStrategy
    {
        private static readonly object _fileLock = new object();
        private readonly string _logDirectory;
        private readonly IFormatter _formatter;
        private readonly string _userName;
        private static readonly HttpClient _httpClient = new HttpClient();

        public string Destination { get; set; } = "Both";
        public string ServerUrl { get; set; }

        public LogDaily(string logDirectory, IFormatter formatter, string serverUrl, string userName)
        {
            _logDirectory = logDirectory;
            _formatter = formatter;
            ServerUrl = serverUrl;
            _userName = userName;

            if (!Directory.Exists(_logDirectory)) Directory.CreateDirectory(_logDirectory);
        }

        public void WriteLog(LogModel logModel)
        {
            logModel.userName = _userName;
            string serializedLog = _formatter.Serialize(logModel);

            if (Destination == "Local" || Destination == "Both")
            {
                WriteLocal(serializedLog);
            }

            if (Destination == "Centralized" || Destination == "Both")
            {
                SendToServer(serializedLog);
            }
        }

        private void WriteLocal(string serializedLog)
        {
            string ext = serializedLog.TrimStart().StartsWith("<") ? "xml" : "json";
            string filePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.{ext}");

            lock (_fileLock)
            {
                bool addComma = ext == "json" && File.Exists(filePath) && new FileInfo(filePath).Length > 0;
                using var writer = new StreamWriter(filePath, true);
                if (addComma) writer.WriteLine(",");
                writer.WriteLine(serializedLog);
            }
        }

        private async void SendToServer(string serializedLog)
        {
            try
            {
                if (string.IsNullOrEmpty(ServerUrl)) return;

                string mediaType = serializedLog.TrimStart().StartsWith("<") ? "application/xml" : "application/json";
                var content = new StringContent(serializedLog, Encoding.UTF8, mediaType);

                await _httpClient.PostAsync(ServerUrl, content);
            }
            catch { }
        }

        public void RemoveLog(string name)
        {
        }
    }
}