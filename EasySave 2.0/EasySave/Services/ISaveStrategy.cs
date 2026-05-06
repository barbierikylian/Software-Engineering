using EasyLog;
using EasySave.Model;
using System;

namespace EasySave.Service
{
    public interface ISaveStrategy
    {
        string Save(Backup job, string businessSoftware, ILogStrategy logger, IFormatter formatter, IProgress<int> progress = null, Action<string> currentFileCallback = null);
    }
}