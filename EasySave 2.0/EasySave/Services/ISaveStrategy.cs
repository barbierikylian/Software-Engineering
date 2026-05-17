using EasySave.Model;

namespace EasySave.Services
{
    public interface ISaveStrategy
    {
        string Save(Backup job, string businessSoftware, string encryptedExtensions, EasyLog.ILogStrategy logger, EasyLog.IFormatter formatter, System.IProgress<int> progress = null, System.Action<string> currentFileCallback = null);
    }
}