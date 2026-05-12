using EasyLog;
using EasySave.Model;
using EasySave.Service;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Services
{
    public class SaveDifferential : ISaveStrategy
    {
        private int _filesCopied = 0;
        private long _bytesCopied = 0;
        private static readonly object _logLock = new object();

        public async Task<string> SaveAsync(Backup job, string businessSoftware, string encryptedExtensions, string priorityExtensions, ILogStrategy logger, IFormatter formatter, IProgress<int> progress, Action<string> currentFileCallback, CancellationToken cancelToken, ManualResetEventSlim pauseEvent)
        {
            LogModel state = new LogModel
            {
                name = job.Name,
                state = "Active",
                progression = 0,
                time = DateTime.Now
            };

            try
            {
                string source = SaveServices.ConvertToUNC(job.FileSource);
                string target = SaveServices.ConvertToUNC(job.FileDestination);

                if (!Directory.Exists(source))
                {
                    lock (_logLock)
                    {
                        state.state = "Error";
                        state.time = DateTime.Now;
                        logger.WriteLog(state);
                    }
                    return "Error: Source directory does not exist.";
                }

                _filesCopied = 0;
                _bytesCopied = 0;

                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string logDir = Path.Combine(appData, "EasySave", "logs");
                ILogStrategy dailyLogger = new LogDaily(logDir, formatter);

                var allFiles = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
                long totalSize = allFiles.Sum(f => new FileInfo(f).Length);

                state.totalFilesToCopy = allFiles.Length;
                state.totalFilesSize = totalSize;
                state.nbFilesLeftToDo = allFiles.Length;
                state.sizeFileRemaining = totalSize;

                bool wasCanceled = false;

                await Task.Run(() =>
                {
                    try
                    {
                        CopyFiles(source, target, allFiles, businessSoftware, encryptedExtensions, priorityExtensions, state, logger, dailyLogger, progress, currentFileCallback, cancelToken, pauseEvent);
                    }
                    catch (OperationCanceledException)
                    {
                        wasCanceled = true;
                    }
                });

                if (wasCanceled)
                {
                    lock (_logLock)
                    {
                        state.state = "Stopped";
                        state.time = DateTime.Now;
                        logger.WriteLog(state);
                    }
                    return "Job stopped.";
                }

                lock (_logLock)
                {
                    state.state = "End"; state.progression = 100; state.nbFilesLeftToDo = 0;
                    state.sizeFileRemaining = 0; state.time = DateTime.Now;
                    logger.WriteLog(state);
                }
                return null;
            }
            catch (Exception ex)
            {
                lock (_logLock)
                {
                    state.state = "Error";
                    state.time = DateTime.Now;
                    logger.WriteLog(state);
                }
                return $"Error: {ex.Message}";
            }
        }

        private void CopyFiles(string source, string target, string[] allFiles, string businessSoftware, string encryptedExtensions, string priorityExtensions, LogModel state, ILogStrategy logger, ILogStrategy dailyLogger, IProgress<int> progress, Action<string> currentFileCallback, CancellationToken cancelToken, ManualResetEventSlim pauseEvent)
        {
            string[] prioExts = (priorityExtensions ?? "").Split(';').Select(e => e.Trim().ToLower()).Where(e => !string.IsNullOrEmpty(e)).Select(e => e.StartsWith(".") ? e : "." + e).ToArray();
            var sortedFiles = allFiles.OrderByDescending(f => prioExts.Contains(Path.GetExtension(f).ToLower())).ToArray();

            foreach (string filePath in sortedFiles)
            {
                cancelToken.ThrowIfCancellationRequested();
                pauseEvent.Wait(cancelToken);

                bool wasBlockedBetween = false;
                while (BusinessSoftwareDetector.IsRunning(businessSoftware))
                {
                    cancelToken.ThrowIfCancellationRequested();
                    if (!wasBlockedBetween)
                    {
                        lock (_logLock) { state.state = "Paused (Business Software)"; logger.WriteLog(state); }
                        currentFileCallback?.Invoke($"⏸ Blocked by process: {businessSoftware}");
                        wasBlockedBetween = true;
                    }
                    Thread.Sleep(2000);
                }
                if (wasBlockedBetween)
                {
                    lock (_logLock) { state.state = "Active"; }
                }

                string relativePath = filePath.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar);
                string destPath = Path.Combine(target, relativePath);

                if (ShouldCopy(filePath, destPath))
                {
                    string destDir = Path.GetDirectoryName(destPath);
                    if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                    long fileSize = new FileInfo(filePath).Length;
                    bool isBigFile = fileSize > SaveServices.BIG_FILE_THRESHOLD;
                    bool semaphoreAcquired = false;

                    try
                    {
                        if (isBigFile)
                        {
                            SaveServices.BigFileSemaphore.Wait(cancelToken);
                            semaphoreAcquired = true;
                        }
                        Stopwatch fileTimer = Stopwatch.StartNew();

                        long encTime = SaveServices.CopyOrEncrypt(filePath, destPath, encryptedExtensions, (chunkSize) =>
                        {
                            bool wasPausedBySoftware = false;
                            while (BusinessSoftwareDetector.IsRunning(businessSoftware))
                            {
                                if (!wasPausedBySoftware)
                                {
                                    lock (_logLock) { state.state = "Paused (Business Software)"; logger.WriteLog(state); }
                                    currentFileCallback?.Invoke($"⏸ Blocked by process: {businessSoftware}");
                                    wasPausedBySoftware = true;
                                }
                                cancelToken.ThrowIfCancellationRequested();
                                Thread.Sleep(1000);
                            }
                            if (wasPausedBySoftware)
                            {
                                lock (_logLock) { state.state = "Active"; logger.WriteLog(state); }
                            }

                            lock (_logLock)
                            {
                                _bytesCopied += chunkSize;
                                if (state.totalFilesSize > 0)
                                    state.progression = (int)((_bytesCopied * 100) / state.totalFilesSize);
                                state.sizeFileRemaining = state.totalFilesSize - _bytesCopied;
                                state.time = DateTime.Now;
                            }
                            progress?.Report(state.progression ?? 0);
                            currentFileCallback?.Invoke($"Updating: {Path.GetFileName(filePath)} ({state.progression}%)");
                        }, pauseEvent, cancelToken);

                        fileTimer.Stop();
                        _filesCopied++;

                        lock (_logLock)
                        {
                            state.nbFilesLeftToDo = state.totalFilesToCopy - _filesCopied;
                            logger.WriteLog(state);
                            dailyLogger.WriteLog(new LogModel
                            {
                                name = state.name,
                                fileSource = filePath,
                                fileDestination = destPath,
                                fileSize = fileSize,
                                fileTransferTime = fileTimer.Elapsed.TotalMilliseconds,
                                encryptionTime = encTime,
                                time = DateTime.Now
                            });
                        }
                    }
                    finally { if (semaphoreAcquired) SaveServices.BigFileSemaphore.Release(); }
                }
                else
                {
                    _filesCopied++;
                    lock (_logLock) { state.nbFilesLeftToDo = state.totalFilesToCopy - _filesCopied; }
                }
            }
        }

        private bool ShouldCopy(string sourceFile, string destFile)
        {
            if (!File.Exists(destFile)) return true;
            return File.GetLastWriteTime(sourceFile) > File.GetLastWriteTime(destFile);
        }
    }
}