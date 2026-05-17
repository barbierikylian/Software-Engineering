using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Xml.Linq;

namespace EasyLog
{
    public class LogLive : ILogStrategy
    {
        private static readonly object _fileLock = new object();
        private readonly string _filePath;
        private readonly IFormatter _formatter;

        public LogLive(string filePath) : this(filePath, new JsonFormatter())
        {
        }

        public LogLive(string filePath, IFormatter formatter)
        {
            _formatter = formatter;
            _filePath = Path.ChangeExtension(filePath, formatter.FileExtension);

            string directory = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrEmpty(directory) == false)
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
                }
                else
                {
                    WriteJson(logModel);
                }
            }
        }

        public void RemoveLog(string jobName)
        {
            lock (_fileLock)
            {
                if (_formatter.FileExtension == "xml")
                {
                    RemoveXml(jobName);
                }
                else
                {
                    RemoveJson(jobName);
                }
            }
        }

        private void RemoveXml(string jobName)
        {
            if (File.Exists(_filePath) == false)
            {
                return;
            }

            try
            {
                string fileContent = SafeReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    return;
                }

                if (fileContent.Contains("<Logs>") == false)
                {
                    return;
                }

                XDocument doc = XDocument.Parse(fileContent);
                XElement existingEntry = null;

                if (doc.Root != null)
                {
                    foreach (XElement element in doc.Root.Elements())
                    {
                        XElement nameElement = element.Element("name");
                        if (nameElement != null)
                        {
                            if (nameElement.Value == jobName)
                            {
                                existingEntry = element;
                                break;
                            }
                        }
                    }
                }

                if (existingEntry != null)
                {
                    existingEntry.Remove();
                    string finalXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + doc.ToString();
                    SafeWriteAllText(_filePath, finalXml);
                }
            }
            catch
            {
            }
        }

        private void RemoveJson(string jobName)
        {
            if (File.Exists(_filePath) == false)
            {
                return;
            }

            List<LogModel> currentStates = new List<LogModel>();

            try
            {
                string content = SafeReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(content) == false)
                {
                    List<LogModel> deserialized = JsonSerializer.Deserialize<List<LogModel>>(content);
                    if (deserialized != null)
                    {
                        currentStates = deserialized;
                    }
                }
            }
            catch
            {
                return;
            }

            int removedCount = 0;
            List<LogModel> elementsToRemove = new List<LogModel>();

            foreach (LogModel state in currentStates)
            {
                if (state.name == jobName)
                {
                    elementsToRemove.Add(state);
                }
            }

            foreach (LogModel stateToRemove in elementsToRemove)
            {
                currentStates.Remove(stateToRemove);
                removedCount++;
            }

            if (removedCount > 0)
            {
                List<Dictionary<string, object>> listToSerialize = new List<Dictionary<string, object>>();
                foreach (LogModel state in currentStates)
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();

                    if (state.name != null) dict.Add("name", state.name);
                    if (state.state != null) dict.Add("state", state.state);
                    if (state.totalFilesToCopy != null) dict.Add("totalFilesToCopy", state.totalFilesToCopy);
                    if (state.totalFilesSize != null) dict.Add("totalFilesSize", state.totalFilesSize);
                    if (state.nbFilesLeftToDo != null) dict.Add("nbFilesLeftToDo", state.nbFilesLeftToDo);
                    if (state.sizeFileRemaining != null) dict.Add("sizeFileRemaining", state.sizeFileRemaining);
                    if (state.progression != null) dict.Add("progression", state.progression);
                    if (state.time != null) dict.Add("time", state.time);
                    if (state.currentSourceFile != null) dict.Add("currentSourceFile", state.currentSourceFile);
                    if (state.currentDestinationFile != null) dict.Add("currentDestinationFile", state.currentDestinationFile);

                    listToSerialize.Add(dict);
                }

                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                string jsonOutput = JsonSerializer.Serialize(listToSerialize, options);
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
                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    XElement rootElement = new XElement("Logs");
                    doc = new XDocument(rootElement);
                }
                else if (fileContent.Contains("<Logs>") == false)
                {
                    XElement rootElement = new XElement("Logs");
                    doc = new XDocument(rootElement);
                }
                else
                {
                    doc = XDocument.Parse(fileContent);
                }

                XElement newElement = XElement.Parse(newEntryXml);
                List<XElement> emptyElements = new List<XElement>();

                foreach (XElement child in newElement.Elements())
                {
                    if (string.IsNullOrWhiteSpace(child.Value))
                    {
                        emptyElements.Add(child);
                    }
                }

                foreach (XElement emptyChild in emptyElements)
                {
                    emptyChild.Remove();
                }

                XElement existingEntry = null;

                if (doc.Root != null)
                {
                    foreach (XElement element in doc.Root.Elements())
                    {
                        XElement nameElement = element.Element("name");
                        if (nameElement != null)
                        {
                            if (nameElement.Value == logModel.name)
                            {
                                existingEntry = element;
                                break;
                            }
                        }
                    }
                }

                if (existingEntry != null)
                {
                    existingEntry.ReplaceWith(newElement);
                }
                else
                {
                    if (doc.Root != null)
                    {
                        doc.Root.Add(newElement);
                    }
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
                    if (string.IsNullOrWhiteSpace(content) == false)
                    {
                        List<LogModel> deserialized = JsonSerializer.Deserialize<List<LogModel>>(content);
                        if (deserialized != null)
                        {
                            currentStates = deserialized;
                        }
                    }
                }
                catch
                {
                    currentStates = new List<LogModel>();
                }
            }

            int index = -1;
            for (int i = 0; i < currentStates.Count; i++)
            {
                if (currentStates[i].name == logModel.name)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                currentStates[index] = logModel;
            }
            else
            {
                currentStates.Add(logModel);
            }

            List<Dictionary<string, object>> listToSerialize = new List<Dictionary<string, object>>();
            foreach (LogModel state in currentStates)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();

                if (state.name != null) dict.Add("name", state.name);
                if (state.state != null) dict.Add("state", state.state);
                if (state.totalFilesToCopy != null) dict.Add("totalFilesToCopy", state.totalFilesToCopy);
                if (state.totalFilesSize != null) dict.Add("totalFilesSize", state.totalFilesSize);
                if (state.nbFilesLeftToDo != null) dict.Add("nbFilesLeftToDo", state.nbFilesLeftToDo);
                if (state.sizeFileRemaining != null) dict.Add("sizeFileRemaining", state.sizeFileRemaining);
                if (state.progression != null) dict.Add("progression", state.progression);
                if (state.time != null) dict.Add("time", state.time);
                if (state.currentSourceFile != null) dict.Add("currentSourceFile", state.currentSourceFile);
                if (state.currentDestinationFile != null) dict.Add("currentDestinationFile", state.currentDestinationFile);

                listToSerialize.Add(dict);
            }

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            string jsonOutput = JsonSerializer.Serialize(listToSerialize, options);
            SafeWriteAllText(_filePath, jsonOutput);
        }

        private string SafeReadAllText(string path)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            return sr.ReadToEnd();
                        }
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
                    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.Write(content);
                            return;
                        }
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