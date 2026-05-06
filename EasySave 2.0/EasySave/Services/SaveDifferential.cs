using EasyLog;
using EasySave.Model;
using EasySave.Service;
using System;
using System.Diagnostics;
using System.IO;

namespace EasySave.Services
{
    public class SaveDifferential : ISaveStrategy
    {
        private int _filesCopied = 0;
        private long _bytesCopied = 0;

        public string Save(Backup job, string businessSoftware, ILogStrategy logger, IFormatter formatter, IProgress<int> progress = null, Action<string> currentFileCallback = null)
        {
            try
            {
                string source = SaveServices.ConvertToUNC(job.FileSource);
                string target = SaveServices.ConvertToUNC(job.FileDestination);

                if (!Directory.Exists(source)) return "Error: Source directory does not exist.";

                _filesCopied = 0;
                _bytesCopied = 0;

                LogModel state = new LogModel
                {
                    name = job.Name,
                    state = "Active",
                    totalFilesToCopy = Directory.GetFiles(source, "*", SearchOption.AllDirectories).Length
                };

                Stopwatch timer = Stopwatch.StartNew();

                string recursiveError = CopyDirectoryRecursive(source, target, businessSoftware, state, logger, progress, currentFileCallback);

                timer.Stop();

                if (recursiveError != null)
                {
                    return recursiveError;
                }

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

                return null;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private string CopyDirectoryRecursive(string src, string trg, string businessSoftware, LogModel state, ILogStrategy logger, IProgress<int> progress, Action<string> currentFileCallback)
        {
            if (!Directory.Exists(trg)) Directory.CreateDirectory(trg);

            foreach (string filePath in Directory.GetFiles(src))
            {
                if (BusinessSoftwareDetector.IsRunning(businessSoftware))
                {
                    state.state = "Stopped - Business Software Detected";
                    logger.WriteLog(state);
                    return $"Backup stopped: Business software ({businessSoftware}) detected.";
                }

                string destPath = Path.Combine(trg, Path.GetFileName(filePath));

                if (ShouldCopy(filePath, destPath))
                {
                    bool isNewFile = !File.Exists(destPath);
                    long fileSize = new FileInfo(filePath).Length;

                    state.currentSourceFile = filePath;
                    state.currentDestinationFile = destPath;

                    SaveServices.CopyOrEncrypt(filePath, destPath);

                    _filesCopied++;
                    _bytesCopied += fileSize;

                    state.nbFilesLeftToDo = state.totalFilesToCopy - _filesCopied;

                    if (state.totalFilesSize > 0)
                        state.progression = (int)((_bytesCopied * 100) / state.totalFilesSize);

                    logger.WriteLog(state);

                    progress?.Report(state.progression ?? 0);

                    string actionType = isNewFile ? "Added" : "Updated";
                    currentFileCallback?.Invoke($"{actionType}: {Path.GetFileName(filePath)} ({state.progression ?? 0}%)");
                }
            }

            foreach (string dirPath in Directory.GetDirectories(src))
            {
                string error = CopyDirectoryRecursive(dirPath, Path.Combine(trg, Path.GetFileName(dirPath)), businessSoftware, state, logger, progress, currentFileCallback);

                if (error != null) return error;
            }

            return null;
        }

        private bool ShouldCopy(string sourceFile, string destFile)
        {
            if (!File.Exists(destFile)) return true;
            return File.GetLastWriteTime(sourceFile) > File.GetLastWriteTime(destFile);
        }
    }
}