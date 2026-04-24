using System;
using System.IO;
using System.Diagnostics;
using EasySave.Model;
using EasyLog;

namespace EasySave.Services
{
    // Full backup: copies every file, regardless of modification date.
    public class SaveComplete : ISaveStrategy
    {
        private int _filesCopied = 0;
        private long _bytesCopied = 0;

        public void Save(Backup job, LogModel state, ILogStrategy liveLogger)
        {
            try
            {
                string source = SaveServices.ConvertToUNC(job.FileSource);
                string target = SaveServices.ConvertToUNC(job.FileDestination);

                if (!Directory.Exists(source)) return;

                _filesCopied = 0;
                _bytesCopied = 0;

                Stopwatch timer = Stopwatch.StartNew();

                CopyDirectoryRecursive(source, target, state, liveLogger);

                timer.Stop();

                // Write one daily log entry per job (summary)
                LogModel dailyLog = new LogModel
                {
                    name = job.Name,
                    fileSource = source,
                    fileDestination = target,
                    fileSize = _bytesCopied,
                    fileTransferTime = timer.Elapsed.TotalMilliseconds,
                    time = DateTime.Now
                };

                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string logDir = Path.Combine(appData, "EasySave", "logs");

                ILogStrategy dailyLogger = new LogDaily(logDir);
                dailyLogger.WriteLog(dailyLog);

                Console.WriteLine($"\nSave {job.Name} finished in {timer.Elapsed.TotalMilliseconds} ms.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error : {ex.Message}");
            }
        }

        // Recursively walks through src and copies every file into trg, updating the live state after each copy.
        private void CopyDirectoryRecursive(string src, string trg, LogModel state, ILogStrategy logger)
        {
            if (!Directory.Exists(trg)) Directory.CreateDirectory(trg);

            foreach (string filePath in Directory.GetFiles(src))
            {
                string destPath = Path.Combine(trg, Path.GetFileName(filePath));
                long fileSize = new FileInfo(filePath).Length;

                SaveServices.CopyFile(filePath, destPath);

                _filesCopied++;
                _bytesCopied += fileSize;

                state.nbFilesLeftToDo = state.totalFilesToCopy - _filesCopied;
                if (state.totalFilesSize > 0)
                    state.progression = (int)((_bytesCopied * 100) / state.totalFilesSize);

                logger.WriteLog(state);

                Console.Write($"\rProgress: {state.progression}% | Files left: {state.nbFilesLeftToDo}    ");
            }

            foreach (string dirPath in Directory.GetDirectories(src))
            {
                CopyDirectoryRecursive(dirPath, Path.Combine(trg, Path.GetFileName(dirPath)), state, logger);
            }
        }
    }
}