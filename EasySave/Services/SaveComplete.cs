using System;
using System.IO;
using EasySave.Model;

namespace EasySave.Services
{
    public class SaveComplete : ISaveStrategy
    {
        public void Save(Backup job)
        {
            try
            {
                string source = SaveServices.ConvertToUNC(job.SourcePath);
                string target = SaveServices.ConvertToUNC(job.TargetPath);

                if (!Directory.Exists(source))
                {
                    Console.WriteLine($"error : file {source} doesnt exist");
                    return;
                }

                Console.WriteLine($"start of recursive save for : {job.Name}");

                CopyDirectoryRecursive(source, target);

                Console.WriteLine($"save {job.Name} finish");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error : {ex.Message}");
            }
        }

        private void CopyDirectoryRecursive(string currentSource, string currentTarget)
        {

            if (!Directory.Exists(currentTarget))
            {
                Directory.CreateDirectory(currentTarget);
            }

            foreach (string filePath in Directory.GetFiles(currentSource))
            {
                string fileName = Path.GetFileName(filePath);
                string destPath = Path.Combine(currentTarget, fileName);

                SaveServices.CopyFile(filePath, destPath);
            }

            foreach (string directoryPath in Directory.GetDirectories(currentSource))
            {
                string folderName = Path.GetFileName(directoryPath);
                string nextTarget = Path.Combine(currentTarget, folderName);

                CopyDirectoryRecursive(directoryPath, nextTarget);
            }
        }
    }
}