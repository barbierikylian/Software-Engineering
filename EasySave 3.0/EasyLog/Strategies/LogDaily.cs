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
            string ext = "json";
            string formatterName = _formatter.GetType().Name.ToLower();
            if (formatterName.Contains("xml"))
            {
                ext = "xml";
            }

            string dateString = DateTime.Now.ToString("yyyy-MM-dd");
            string fileName = dateString + "." + ext;
            string filePath = Path.Combine(_logDirectory, fileName);

            lock (_fileLock)
            {
                try
                {
                    bool addComma = false;
                    if (ext == "json")
                    {
                        if (File.Exists(filePath))
                        {
                            FileInfo fileInfo = new FileInfo(filePath);
                            if (fileInfo.Length > 0)
                            {
                                addComma = true;
                            }
                        }
                    }

                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        if (addComma)
                        {
                            writer.WriteLine(",");
                        }
                        writer.WriteLine(serializedLog);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error writing local log: " + ex.Message);
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