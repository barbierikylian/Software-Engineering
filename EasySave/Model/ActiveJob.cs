namespace EasySave.Model
{
    public class ActiveJob
    {
        public string NameFile { get; set; } = string.Empty;
        public long SizeFile { get; set; }
        public int Progression { get; set; }
        public int FileRemaining { get; set; }
        public long SizeFileRemaining { get; set; }
    }
}