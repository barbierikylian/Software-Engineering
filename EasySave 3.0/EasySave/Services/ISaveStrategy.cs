using EasyLog;
using EasySave.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Service
{
    public interface ISaveStrategy
    {
        Task<string> SaveAsync(Backup job, string businessSoftware, string encryptedExtensions, string priorityExtensions, long maxFileSizeBytes, string logDestination, string serverUrl, ILogStrategy logger, IFormatter formatter, IProgress<int> progress, Action<string> currentFileCallback, CancellationToken cancelToken, ManualResetEventSlim pauseEvent);
    }
}