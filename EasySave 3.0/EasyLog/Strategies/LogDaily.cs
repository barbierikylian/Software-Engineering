using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasyLog
{
    public class LogDaily : ILogStrategy
    {
        private static readonly object _fileLock = new object();
        private readonly string _logDirectory;
        private readonly IFormatter _formatter;

        public string Destination { get; set; } = "Both";
        public string ServerUrl { get; set; }

        private static readonly HttpClient _httpClient = new HttpClient();

        public LogDaily(string logDirectory, IFormatter formatter, string serverUrl)
        {
            _logDirectory = logDirectory;
            _formatter = formatter;
            ServerUrl = serverUrl;

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void WriteLog(LogModel logModel)
        {
            string serializedLog = _formatter.Serialize(logModel);

            if (Destination == "Local" || Destination == "Both")
            {
                WriteLocal(serializedLog);
            }

            if (Destination == "Centralized" || Destination == "Both")
            {
                SendToDocker(serializedLog);
            }
        }

        public void RemoveLog(string jobName) { }

        private void WriteLocal(string serializedLog)
        {
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.{_formatter.FileExtension}";
            string filePath = Path.Combine(_logDirectory, fileName);

            lock (_fileLock)
            {
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        if (_formatter.FileExtension == "xml") WriteXml(filePath, serializedLog);
                        else WriteJson(filePath, serializedLog);
                        return;
                    }
                    catch (IOException) { Thread.Sleep(100); }
                }
            }
        }

        private void WriteJson(string filePath, string serializedLog)
        {
            bool addComma = File.Exists(filePath) && new FileInfo(filePath).Length > 0;
            using var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var sw = new StreamWriter(fs);
            if (addComma) sw.WriteLine(",");
            sw.Write(serializedLog);
        }

        private void WriteXml(string filePath, string serializedLog)
        {
            try
            {
                XDocument doc;
                if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0) doc = new XDocument(new XElement("Logs"));
                else doc = XDocument.Parse(File.ReadAllText(filePath));

                doc.Root?.Add(XElement.Parse(serializedLog));
                File.WriteAllText(filePath, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + doc.ToString());
            }
            catch
            {
                File.AppendAllText(filePath, serializedLog + Environment.NewLine);
            }
        }

        private void SendToDocker(string serializedLog)
        {
            Task.Run(async () =>
            {
                try
                {
                    string mediaType = _formatter.FileExtension == "xml" ? "application/xml" : "application/json";
                    var content = new StringContent(serializedLog, Encoding.UTF8, mediaType);
                    await _httpClient.PostAsync(ServerUrl, content);
                }
                catch { }
            });
        }
    }
}