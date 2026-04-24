using System;
using System.IO;
using EasySave.Model;

namespace EasySave.Services
{
    public class SaveDifferential : ISaveStrategy
    {
        public void Save(Backup job)
        {

            try
            {
                string source = SaveServices.ConvertToUNC(job.SourcePath);
                string target = SaveServices.ConvertToUNC(job.TargetPath);

                if (!Directory.Exists(source)) return;

                Console.WriteLine($"differential save : {job.Name}");
                CopyDirectoryRecursive(source, target);
                Console.WriteLine($"save {job.Name} finish.");
            }
            catch (Exception ex) { Console.WriteLine($"error : {ex.Message}"); }
        }

        private void CopyDirectoryRecursive(string currentSource, string currentTarget)
        {
            if (!Directory.Exists(currentTarget)) Directory.CreateDirectory(currentTarget);


            foreach (string filePath in Directory.GetFiles(currentSource))
            {
                string fileName = Path.GetFileName(filePath);
                string destPath = Path.Combine(currentTarget, fileName);

                if (ShouldCopy(filePath, destPath))
                {
                    SaveServices.CopyFile(filePath, destPath);
                }
            }

            foreach (string directoryPath in Directory.GetDirectories(currentSource))
            {
                string folderName = Path.GetFileName(directoryPath);
                string nextTarget = Path.Combine(currentTarget, folderName);
                CopyDirectoryRecursiveIncremental(directoryPath, nextTarget);
            }
        }


        private bool ShouldCopy(string sourceFile, string destFile)
        {

            if (!File.Exists(destFile)) return true;

            DateTime sourceTime = File.GetLastWriteTime(sourceFile);
            DateTime destTime = File.GetLastWriteTime(destFile);

            return sourceTime > destTime;
        }
    }
}