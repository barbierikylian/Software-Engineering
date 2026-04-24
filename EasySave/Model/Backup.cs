using System;

namespace EasySave.Model
{
    public class Backup
    {

        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public string Type { get; set; }

        public Backup(string name, string source, string target, string type)
        {
            Name = name;
            SourcePath = source;
            TargetPath = target;
            Type = type;
        }
    }
}