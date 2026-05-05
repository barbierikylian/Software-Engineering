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
            File.AppendAllText(fullPath, text + "\n");
        }
    }
}