using System;

namespace EasyLog
{
    public interface ILogStrategy
    {
        // Strategy pattern interface for logging. Implemented by LogDaily and LogLive.
        void WriteLog(LogModel logModel);
    }
}