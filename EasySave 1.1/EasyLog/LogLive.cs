namespace EasyLog
{
    
    public class LogLive : ILogStrategy
    {
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
            string text = _formatter.Serialize(logModel);
            File.WriteAllText(_filePath, text);
        }
    }
}