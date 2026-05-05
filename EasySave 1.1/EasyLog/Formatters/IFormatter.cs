using System;

namespace EasyLog
{
    
    public interface IFormatter
    {
        string Serialize(LogModel logModel);

        
        string FileExtension { get; }
    }
}