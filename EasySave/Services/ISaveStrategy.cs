using EasyLog;
using EasySave.Model;
using System;

namespace EasySave.Services
{
    // Strategy pattern interface for backup execution. Implemented by SaveComplete and SaveDifferential.
    public interface ISaveStrategy
    {
        void Save(Backup job, LogModel state, ILogStrategy logger);
    }
}