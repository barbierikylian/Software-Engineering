namespace EasySave.Model
{
    // Configuration of a backup job (name, source, destination, type).
    public class Backup
    {
        public string Name { get; set; } = string.Empty;
        public string FileSource { get; set; } = string.Empty;
        public string FileDestination { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}