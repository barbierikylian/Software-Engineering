using System;

namespace EasyLog
{
    public interface ILogStrategy
    {
        void WriteLog(LogModel logModel);
    }
}