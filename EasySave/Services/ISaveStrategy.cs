using System;
using EasySave.Model;

namespace EasySave.Services
{
    public interface ISaveStrategy
    {
        void Save(Backup job);
    }
}