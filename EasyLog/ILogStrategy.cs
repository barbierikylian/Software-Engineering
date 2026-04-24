using System;

namespace EasyLog
{
    // Strategy pattern interface for logging. Implemented by LogDaily and LogLive.
    public interface ILogStrategy
    {
        void WriteLog(LogModel logModel);
    }
}