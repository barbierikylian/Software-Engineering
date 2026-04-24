using EasyLog;
using EasySave.Model;
using System;

namespace EasySave.Services
{
    public interface ISaveStrategy
    {
        void Save(Backup job, LogModel state, ILogStrategy logger);
    }
}