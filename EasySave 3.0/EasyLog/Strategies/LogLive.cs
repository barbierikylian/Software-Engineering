using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Linq;
using System.Xml.Linq;

namespace EasyLog
{
    public class LogLive : ILogStrategy
    {
        private static readonly object _fileLock = new object();
        private readonly string _filePath;
        private readonly IFormatter _formatter;

        public LogLive(string filePath) : this(filePath, new JsonFormatter()) { }

        public LogLive(string filePath, IFormatter formatter)
        {
            _formatter = formatter;
            _filePath = Path.ChangeExtension(filePath, formatter.FileExtension);

            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void WriteLog(LogModel logModel)
        {
            lock (_fileLock)
            {
                if (_formatter.FileExtension == "xml")
                {
                    WriteXml(logModel);
                    return;
                }

                WriteJson(logModel);
            }
        }

        public void RemoveLog(string jobName)
        {
            lock (_fileLock)
            {
                if (_formatter.FileExtension == "xml")
                {
                    RemoveXml(jobName);
                    return;
                }

                RemoveJson(jobName);
            }
        }

        private void RemoveXml(string jobName)
        {
            if (!File.Exists(_filePath)) return;

            try
            {
                string fileContent = SafeReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(fileContent) || !fileContent.Contains("<Logs>"))
                    return;

                XDocument doc = XDocument.Parse(fileContent);
                var existingEntry = doc.Root?.Elements().FirstOrDefault(e => e.Element("name")?.Value == jobName);

                if (existingEntry != null)
                {
                    existingEntry.Remove();
                    string finalXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + doc.ToString();
                    SafeWriteAllText(_filePath, finalXml);
                }
            }
            catch { }
        }

        private void RemoveJson(string jobName)
        {
            if (!File.Exists(_filePath)) return;

            List<LogModel> currentStates = new List<LogModel>();

            try
            {
                string content = SafeReadAllText(_filePath);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    currentStates = JsonSerializer.Deserialize<List<LogModel>>(content) ?? new List<LogModel>();
                }
            }
            catch
            {
                return;
            }

            int removedCount = currentStates.RemoveAll(s => s.name == jobName);

            if (removedCount > 0)
            {
                var listToSerialize = new List<Dictionary<string, object>>();
                foreach (var state in currentStates)
                {
                    var dict = new Dictionary<string, object>();

                    if (state.name != null) dict["name"] = state.name;
                    if (state.state != null) dict["state"] = state.state;
                    if (state.totalFilesToCopy != null) dict["totalFilesToCopy"] = state.totalFilesToCopy;
                    if (state.totalFilesSize != null) dict["totalFilesSize"] = state.totalFilesSize;
                    if (state.nbFilesLeftToDo != null) dict["nbFilesLeftToDo"] = state.nbFilesLeftToDo;
                    if (state.sizeFileRemaining != null) dict["sizeFileRemaining"] = state.sizeFileRemaining;
                    if (state.progression != null) dict["progression"] = state.progression;
                    if (state.time != null) dict["time"] = state.time;
                    if (state.currentSourceFile != null) dict["currentSourceFile"] = state.currentSourceFile;
                    if (state.currentDestinationFile != null) dict["currentDestinationFile"] = state.currentDestinationFile;

                    listToSerialize.Add(dict);
                }

                string jsonOutput = JsonSerializer.Serialize(listToSerialize, new JsonSerializerOptions { WriteIndented = true });
                SafeWriteAllText(_filePath, jsonOutput);
            }
        }

        private void WriteXml(LogModel logModel)
        {
            string newEntryXml = _formatter.Serialize(logModel);
            string fileContent = SafeReadAllText(_filePath);

            try
            {
                XDocument doc;
                if (string.IsNullOrWhiteSpace(fileContent) || !fileContent.Contains("<Logs>"))
                {
                    doc = new XDocument(new XElement("Logs"));
                }
                else
                {
                    doc = XDocument.Parse(fileContent);
                }

                XElement newElement = XElement.Parse(newEntryXml);

                newElement.Elements().Where(e => string.IsNullOrWhiteSpace(e.Value)).Remove();

                var existingEntry = doc.Root?.Elements().FirstOrDefault(e => e.Element("name")?.Value == logModel.name);

                if (existingEntry != null)
                {
                    existingEntry.ReplaceWith(newElement);
                }
                else
                {
                    doc.Root?.Add(newElement);
                }

                string finalXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + doc.ToString();
                SafeWriteAllText(_filePath, finalXml);
            }
            catch
            {
                string fallbackXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Logs>\n" + newEntryXml + "\n</Logs>";
                SafeWriteAllText(_filePath, fallbackXml);
            }
        }

        private void WriteJson(LogModel logModel)
        {
            List<LogModel> currentStates = new List<LogModel>();

            if (File.Exists(_filePath))
            {
                try
                {
                    string content = SafeReadAllText(_filePath);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        currentStates = JsonSerializer.Deserialize<List<LogModel>>(content) ?? new List<LogModel>();
                    }
                }
                catch
                {
                    currentStates = new List<LogModel>();
                }
            }

            int index = currentStates.FindIndex(s => s.name == logModel.name);
            if (index != -1)
            {
                currentStates[index] = logModel;
            }
            else
            {
                currentStates.Add(logModel);
            }

            var listToSerialize = new List<Dictionary<string, object>>();
            foreach (var state in currentStates)
            {
                var dict = new Dictionary<string, object>();

                if (state.name != null) dict["name"] = state.name;
                if (state.state != null) dict["state"] = state.state;
                if (state.totalFilesToCopy != null) dict["totalFilesToCopy"] = state.totalFilesToCopy;
                if (state.totalFilesSize != null) dict["totalFilesSize"] = state.totalFilesSize;
                if (state.nbFilesLeftToDo != null) dict["nbFilesLeftToDo"] = state.nbFilesLeftToDo;
                if (state.sizeFileRemaining != null) dict["sizeFileRemaining"] = state.sizeFileRemaining;
                if (state.progression != null) dict["progression"] = state.progression;
                if (state.time != null) dict["time"] = state.time;
                if (state.currentSourceFile != null) dict["currentSourceFile"] = state.currentSourceFile;
                if (state.currentDestinationFile != null) dict["currentDestinationFile"] = state.currentDestinationFile;

                listToSerialize.Add(dict);
            }

            string jsonOutput = JsonSerializer.Serialize(listToSerialize, new JsonSerializerOptions { WriteIndented = true });
            SafeWriteAllText(_filePath, jsonOutput);
        }

        private string SafeReadAllText(string path)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs))
                    {
                        return sr.ReadToEnd();
                    }
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                }
            }
            return string.Empty;
        }

        private void SafeWriteAllText(string path, string content)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(content);
                        return;
                    }
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}