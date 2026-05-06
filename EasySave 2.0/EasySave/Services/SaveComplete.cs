using EasyLog;
using EasySave.Model;
using EasySave.Service;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EasySave.Services
{
    public class SaveComplete : ISaveStrategy
    {
        private int _filesCopied = 0;
        private long _bytesCopied = 0;
        private long _totalEncryptionTime = 0;

        public string Save(Backup job, string businessSoftware, string encryptedExtensions, ILogStrategy logger, IFormatter formatter, IProgress<int> progress = null, Action<string> currentFileCallback = null)
        {
            try
            {
                string source = SaveServices.ConvertToUNC(job.FileSource);
                string target = SaveServices.ConvertToUNC(job.FileDestination);

                if (!Directory.Exists(source)) return "Error: Source directory does not exist.";

                _filesCopied = 0;
                _bytesCopied = 0;
                _totalEncryptionTime = 0;

                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string logDir = Path.Combine(appData, "EasySave", "logs");
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

                ILogStrategy dailyLogger = new LogDaily(logDir, formatter);
                var allFiles = Directory.GetFiles(source, "*", SearchOption.AllDirectories);

                LogModel state = new LogModel
                {
                    name = job.Name,
                    state = "Active",
                    totalFilesToCopy = allFiles.Length,
                    totalFilesSize = allFiles.Sum(f => new FileInfo(f).Length),
                    nbFilesLeftToDo = allFiles.Length,
                    progression = 0
                };

                Stopwatch timer = Stopwatch.StartNew();
                string recursiveError = CopyDirectoryRecursive(source, target, businessSoftware, encryptedExtensions, state, logger, progress, currentFileCallback);
                timer.Stop();

                LogModel dailyLog = new LogModel
                {
                    name = job.Name,
                    fileSource = source,
                    fileDestination = target,
                    fileSize = _bytesCopied,
                    fileTransferTime = recursiveError != null ? -1 : timer.Elapsed.TotalMilliseconds,
                    encryptionTime = _totalEncryptionTime,
                    time = DateTime.Now
                };

                dailyLogger.WriteLog(dailyLog);

                if (recursiveError != null) return recursiveError;

                state.state = "End";
                state.progression = 100;
                state.nbFilesLeftToDo = 0;
                logger.WriteLog(state);

                progress?.Report(100);
                currentFileCallback?.Invoke("Finished.");

                return null;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private string CopyDirectoryRecursive(string src, string trg, string businessSoftware, string encryptedExtensions, LogModel state, ILogStrategy logger, IProgress<int> progress, Action<string> currentFileCallback)
        {
            if (!Directory.Exists(trg)) Directory.CreateDirectory(trg);

            foreach (string filePath in Directory.GetFiles(src))
            {
                string destPath = Path.Combine(trg, Path.GetFileName(filePath));
                long fileSize = new FileInfo(filePath).Length;
                state.currentSourceFile = filePath;
                state.currentDestinationFile = destPath;

                try
                {
                    long encTime = SaveServices.CopyOrEncrypt(filePath, destPath, businessSoftware, encryptedExtensions, (softName) =>
                    {
                        state.state = "Stopped";
                        logger.WriteLog(state);
                    });

                    _filesCopied++;
                    _bytesCopied += fileSize;
                    _totalEncryptionTime += encTime;
                    state.nbFilesLeftToDo = state.totalFilesToCopy - _filesCopied;

                    if (state.totalFilesSize > 0)
                        state.progression = (int)((_bytesCopied * 100) / state.totalFilesSize);

                    logger.WriteLog(state);
                    progress?.Report(state.progression ?? 0);
                    currentFileCallback?.Invoke($"Copied: {Path.GetFileName(filePath)} ({state.progression ?? 0}%)");
                }
                catch (OperationCanceledException)
                {
                    return $"Backup stopped: {businessSoftware} detected.";
                }
            }

            foreach (string dirPath in Directory.GetDirectories(src))
            {
                string error = CopyDirectoryRecursive(dirPath, Path.Combine(trg, Path.GetFileName(dirPath)), businessSoftware, encryptedExtensions, state, logger, progress, currentFileCallback);
                if (error != null) return error;
            }

            return null;
        }
    }
}