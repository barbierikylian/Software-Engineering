using System;
using System.IO;
using System.Diagnostics;
using EasySave.Model;
using EasyLog;

namespace EasySave.Services
{
    public class SaveComplete : ISaveStrategy
    {
        private int _filesCopied = 0;
        private long _bytesCopied = 0;

        public void Save(Backup job, LogModel state, ILogStrategy liveLogger, IFormatter formatter, IProgress<int> progress = null, Action<string> currentFileCallback = null)
        {
            try
            {
                string source = SaveServices.ConvertToUNC(job.FileSource);
                string target = SaveServices.ConvertToUNC(job.FileDestination);

                if (!Directory.Exists(source)) return;

                _filesCopied = 0;
                _bytesCopied = 0;

                Stopwatch timer = Stopwatch.StartNew();

                CopyDirectoryRecursive(source, target, state, liveLogger, progress, currentFileCallback);

                timer.Stop();

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

                ILogStrategy dailyLogger = new LogDaily(logDir, formatter);
                dailyLogger.WriteLog(dailyLog);

                progress?.Report(100);
                currentFileCallback?.Invoke("Finished.");
            }
            catch (Exception ex)
            {
                currentFileCallback?.Invoke($"Error : {ex.Message}");
            }
        }

        private void CopyDirectoryRecursive(string src, string trg, LogModel state, ILogStrategy logger, IProgress<int> progress, Action<string> currentFileCallback)
        {
            if (!Directory.Exists(trg)) Directory.CreateDirectory(trg);

            foreach (string filePath in Directory.GetFiles(src))
            {
                string destPath = Path.Combine(trg, Path.GetFileName(filePath));
                long fileSize = new FileInfo(filePath).Length;

                state.currentSourceFile = filePath;
                state.currentDestinationFile = destPath;

                SaveServices.CopyFile(filePath, destPath);

                _filesCopied++;
                _bytesCopied += fileSize;

                state.nbFilesLeftToDo = state.totalFilesToCopy - _filesCopied;
                state.sizeFileRemaining = state.totalFilesSize - _bytesCopied;

                if (state.totalFilesSize > 0)
                    state.progression = (int)((_bytesCopied * 100) / state.totalFilesSize);

                logger.WriteLog(state);

                progress?.Report(state.progression ?? 0);
                currentFileCallback?.Invoke($"Copied : {Path.GetFileName(filePath)} ({state.progression}%)");
            }

            foreach (string dirPath in Directory.GetDirectories(src))
            {
                CopyDirectoryRecursive(dirPath, Path.Combine(trg, Path.GetFileName(dirPath)), state, logger, progress, currentFileCallback);
            }
        }
    }
}