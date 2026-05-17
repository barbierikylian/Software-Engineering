using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Diagnostics;

namespace EasyLog
{
    public class LogDaily : ILogStrategy
    {
        private static readonly object _fileLock = new object();
        private readonly string _logDirectory;
        private readonly IFormatter _formatter;
        private readonly string _userName;
        private static readonly HttpClient _httpClient = new HttpClient();

        public string Destination { get; set; }
        public string ServerUrl { get; set; }

        public LogDaily(string logDirectory, IFormatter formatter, string serverUrl, string userName)
        {
            Destination = "Both";
            _logDirectory = logDirectory;
            _formatter = formatter;
            ServerUrl = serverUrl;
            _userName = userName;

            try
            {
                if (Directory.Exists(_logDirectory) == false)
                {
                    Directory.CreateDirectory(_logDirectory);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error creating directory: " + ex.Message);
            }
        }

        public void WriteLog(LogModel logModel)
        {
            logModel.userName = _userName;
            string serializedLog = _formatter.Serialize(logModel);

            if (Destination == "Local")
            {
                WriteLocal(serializedLog);
            }
            else if (Destination == "Both")
            {
                WriteLocal(serializedLog);
                SendToServer(serializedLog);
            }
            else if (Destination == "Centralized")
            {
                SendToServer(serializedLog);
            }
        }

        private void WriteLocal(string serializedLog)
        {
            string ext = _formatter.FileExtension;

            string dateString = DateTime.Now.ToString("yyyy-MM-dd");
            string fileName = dateString + "." + ext;
            string filePath = Path.Combine(_logDirectory, fileName);

            lock (_fileLock)
            {
                if (Directory.Exists(_logDirectory) == false)
                {
                    Directory.CreateDirectory(_logDirectory);
                }

                if (ext == "xml")
                {
                    WriteXmlEntry(filePath, serializedLog);
                }
                else
                {
                    WriteJsonEntry(filePath, serializedLog);
                }
            }
        }

        private void WriteJsonEntry(string filePath, string jsonEntry)
        {
            const string closingBracket = "]";

            if (File.Exists(filePath) == false)
            {
                string initial = "[\n" + jsonEntry + "\n]";
                File.WriteAllText(filePath, initial, Encoding.UTF8);
            }
            else
            {
                string existing = File.ReadAllText(filePath, Encoding.UTF8).TrimEnd();
                int closeIndex = existing.LastIndexOf(closingBracket);

                if (closeIndex >= 0)
                {
                    string newContent = existing.Substring(0, closeIndex).TrimEnd()
                        + ",\n" + jsonEntry + "\n]";
                    File.WriteAllText(filePath, newContent, Encoding.UTF8);
                }
                else
                {
                    string newContent = "[\n" + jsonEntry + "\n]";
                    File.WriteAllText(filePath, newContent, Encoding.UTF8);
                }
            }
        }

        private void WriteXmlEntry(string filePath, string xmlEntry)
        {
            const string xmlHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            const string rootOpen = "<Logs>";
            const string rootClose = "</Logs>";

            if (File.Exists(filePath) == false)
            {
                string content = xmlHeader + "\n" + rootOpen + "\n" + xmlEntry + "\n" + rootClose;
                File.WriteAllText(filePath, content, Encoding.UTF8);
            }
            else
            {
                string existing = File.ReadAllText(filePath, Encoding.UTF8);
                int closeIndex = existing.LastIndexOf(rootClose);

                if (closeIndex >= 0)
                {
                    string newContent = existing.Substring(0, closeIndex)
                        + xmlEntry + "\n"
                        + rootClose;
                    File.WriteAllText(filePath, newContent, Encoding.UTF8);
                }
                else
                {
                    string newContent = xmlHeader + "\n" + rootOpen + "\n" + xmlEntry + "\n" + rootClose;
                    File.WriteAllText(filePath, newContent, Encoding.UTF8);
                }
            }
        }

        private async void SendToServer(string serializedLog)
        {
            try
            {
                if (string.IsNullOrEmpty(ServerUrl))
                {
                    return;
                }

                string mediaType = "application/json";
                string formatterName = _formatter.GetType().Name.ToLower();
                if (formatterName.Contains("xml"))
                {
                    mediaType = "application/xml";
                }

                StringContent content = new StringContent(serializedLog, Encoding.UTF8, mediaType);
                await _httpClient.PostAsync(ServerUrl, content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error sending log to server: " + ex.Message);
            }
        }

        public void RemoveLog(string name)
        {
        }
    }
}