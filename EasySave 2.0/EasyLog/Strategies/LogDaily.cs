using System;
using System.IO;
using System.Linq;

namespace EasyLog
{
    public class LogDaily : ILogStrategy
    {
        private readonly string _logDirectory;
        private readonly IFormatter _formatter;

        public LogDaily(string path) : this(path, new JsonFormatter()) { }

        public LogDaily(string path, IFormatter formatter)
        {
            _logDirectory = path;
            _formatter = formatter;
            Directory.CreateDirectory(_logDirectory);
        }

        public void WriteLog(LogModel logModel)
        {
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "." + _formatter.FileExtension;
            string fullPath = Path.Combine(_logDirectory, fileName);

            string text = _formatter.Serialize(logModel);

            if (_formatter.FileExtension == "xml")
            {
                if (!File.Exists(fullPath))
                {
                    File.WriteAllText(fullPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Logs>\n" + text + "\n</Logs>");
                }
                else
                {
                    var lines = File.ReadAllLines(fullPath).ToList();
                    if (lines.Count > 0 && lines.Last().Contains("</Logs>"))
                    {
                        lines.RemoveAt(lines.Count - 1);
                    }
                    lines.Add(text);
                    lines.Add("</Logs>");
                    File.WriteAllLines(fullPath, lines);
                }
            }
            else
            {
                File.AppendAllText(fullPath, text + "\n");
            }
        }
    }
}