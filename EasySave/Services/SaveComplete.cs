using System;
using System.IO;
using System.Diagnostics;
using EasySave.Model;
using EasyLog;

namespace EasySave.Services
{
    public class SaveComplete : ISaveStrategy
    {
        public void Save(Backup job)
        {
            try
            {
                string source = SaveServices.ConvertToUNC(job.FileSource);
                string target = SaveServices.ConvertToUNC(job.FileDestination);

                if (!Directory.Exists(source))
                {
                    Console.WriteLine($"error : file {source} doesnt exist");
                    return;
                }

                Console.WriteLine($"start of recursive save for : {job.Name}");

                Stopwatch timer = Stopwatch.StartNew();

                long totalSize = CopyDirectoryRecursive(source, target);

                timer.Stop();

                Console.WriteLine($"save {job.Name} finish in {timer.Elapsed.TotalMilliseconds} ms.");

                LogModel dailyLog = new LogModel
                {
                    name = job.Name,
                    fileSource = source,
                    fileDestination = target,
                    fileSize = totalSize,
                    fileTransferTime = timer.Elapsed.TotalMilliseconds,
                    time = DateTime.Now
                };

                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string logDirectory = Path.Combine(appData, "EasySave", "logs");

                ILogStrategy logger = new LogDaily(logDirectory);
                logger.WriteLog(dailyLog);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error : {ex.Message}");
            }
        }

        private long CopyDirectoryRecursive(string currentSource, string currentTarget)
        {
            long directorySize = 0;

            if (!Directory.Exists(currentTarget))
            {
                Directory.CreateDirectory(currentTarget);
            }

            foreach (string filePath in Directory.GetFiles(currentSource))
            {
                string fileName = Path.GetFileName(filePath);
                string destPath = Path.Combine(currentTarget, fileName);

                FileInfo fileInfo = new FileInfo(filePath);
                directorySize += fileInfo.Length;

                SaveServices.CopyFile(filePath, destPath);
            }

            foreach (string directoryPath in Directory.GetDirectories(currentSource))
            {
                string folderName = Path.GetFileName(directoryPath);
                string nextTarget = Path.Combine(currentTarget, folderName);

                directorySize += CopyDirectoryRecursive(directoryPath, nextTarget);
            }

            return directorySize;
        }
    }
}