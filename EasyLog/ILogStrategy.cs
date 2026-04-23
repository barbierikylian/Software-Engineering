using System;
using System.Collections.Generic;
using System.Text;

namespace EasyLog
{
    public interface ILogStrategy
    {
        void WriteLog(LogModel logModel);
    }
}