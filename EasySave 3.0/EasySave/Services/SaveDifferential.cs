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

        // --- SIGNATURE MISE À JOUR AVEC logDestination et serverUrl ---
        public async Task<string> SaveAsync(Backup job, string businessSoftware, string encryptedExtensions, string priorityExtensions, long maxFileSizeBytes, string logDestination, string serverUrl, ILogStrategy logger, IFormatter formatter, IProgress<int> progress, Action<string> currentFileCallback, CancellationToken cancelToken, ManualResetEventSlim pauseEvent)
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

                // --- ON PASSE L'URL AU LOGGER ICI ---
                LogDaily dailyLogger = new LogDaily(logDir, formatter, serverUrl);
                dailyLogger.Destination = logDestination;

                var allFiles = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
                long totalSize = allFiles.Sum(f => new FileInfo(f).Length);

                state.totalFilesToCopy = allFiles.Length;
                state.totalFilesSize = totalSize;
                state.nbFilesLeftToDo = allFiles.Length;
                state.sizeFileRemaining = totalSize;

                await Task.Run(() => CopyFiles(source, target, allFiles, businessSoftware, encryptedExtensions, priorityExtensions, maxFileSizeBytes, state, logger, dailyLogger, progress, currentFileCallback, cancelToken, pauseEvent));

                if (cancelToken.IsCancellationRequested)
                {
                    lock (_logLock)
                    {
                        logger.RemoveLog(state.name);
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

        private void CopyFiles(string source, string target, string[] allFiles, string businessSoftware, string encryptedExtensions, string priorityExtensions, long maxFileSizeBytes, LogModel state, ILogStrategy logger, ILogStrategy dailyLogger, IProgress<int> progress, Action<string> currentFileCallback, CancellationToken cancelToken, ManualResetEventSlim pauseEvent)
        {
            string[] prioExts = (priorityExtensions ?? "")
                .Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim().ToLower())
                .Where(e => !string.IsNullOrEmpty(e))
                .Select(e => e.StartsWith(".") ? e : "." + e)
                .ToArray();

            var sortedFiles = allFiles.OrderBy(f =>
            {
                string ext = Path.GetExtension(f).ToLower();
                int index = Array.IndexOf(prioExts, ext);
                return index != -1 ? index : int.MaxValue;
            }).ThenBy(f => new FileInfo(f).Length).ToArray();

            int myRemainingPriorityFiles = sortedFiles.Count(f => Array.IndexOf(prioExts, Path.GetExtension(f).ToLower()) != -1);
            Interlocked.Add(ref SaveServices.PendingPriorityFiles, myRemainingPriorityFiles);

            Stopwatch updateTimer = Stopwatch.StartNew();

            try
            {
                foreach (string filePath in sortedFiles)
                {
                    if (cancelToken.IsCancellationRequested) return;

                    bool isPriority = Array.IndexOf(prioExts, Path.GetExtension(filePath).ToLower()) != -1;

                    try
                    {
                        pauseEvent.Wait(cancelToken);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }

                    bool wasBlockedBetween = false;
                    while (BusinessSoftwareDetector.IsRunning(businessSoftware))
                    {
                        if (cancelToken.IsCancellationRequested) return;
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

                    if (!isPriority)
                    {
                        bool wasPausedForPriority = false;
                        while (Interlocked.Read(ref SaveServices.PendingPriorityFiles) > 0)
                        {
                            if (cancelToken.IsCancellationRequested) return;
                            if (!wasPausedForPriority)
                            {
                                lock (_logLock) { state.state = "Paused (Priority Waiting)"; logger.WriteLog(state); }
                                currentFileCallback?.Invoke($"⏸ Auto Pause: Waiting for priority jobs");
                                wasPausedForPriority = true;
                            }
                            Thread.Sleep(1000);
                        }
                        if (wasPausedForPriority)
                        {
                            lock (_logLock) { state.state = "Active"; logger.WriteLog(state); }
                        }
                    }

                    try
                    {
                        string relativePath = filePath.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar);
                        string destPath = Path.Combine(target, relativePath);

                        if (ShouldCopy(filePath, destPath))
                        {
                            string destDir = Path.GetDirectoryName(destPath);
                            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                            long fileSize = new FileInfo(filePath).Length;
                            bool isBigFile = fileSize > maxFileSizeBytes;
                            bool semaphoreAcquired = false;

                            try
                            {
                                if (isBigFile)
                                {
                                    if (!SaveServices.BigFileSemaphore.Wait(0))
                                    {
                                        lock (_logLock)
                                        {
                                            state.state = "Paused (Big File Limit)";
                                            logger.WriteLog(state);
                                        }
                                        currentFileCallback?.Invoke($"⏸ Auto Pause: {Path.GetFileName(filePath)}");

                                        SaveServices.BigFileSemaphore.Wait(cancelToken);

                                        lock (_logLock) { state.state = "Active"; }
                                    }
                                    semaphoreAcquired = true;
                                }

                                Stopwatch fileTimer = Stopwatch.StartNew();

                                long encTime = SaveServices.CopyOrEncrypt(filePath, destPath, encryptedExtensions, (chunkSize) =>
                                {
                                    bool wasPausedBySoftware = false;
                                    while (BusinessSoftwareDetector.IsRunning(businessSoftware))
                                    {
                                        if (cancelToken.IsCancellationRequested) return;
                                        if (!wasPausedBySoftware)
                                        {
                                            lock (_logLock) { state.state = "Paused (Business Software)"; logger.WriteLog(state); }
                                            currentFileCallback?.Invoke($"⏸ Blocked by process: {businessSoftware}");
                                            wasPausedBySoftware = true;

                                            if (isBigFile && semaphoreAcquired)
                                            {
                                                SaveServices.BigFileSemaphore.Release();
                                                semaphoreAcquired = false;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    if (wasPausedBySoftware)
                                    {
                                        if (isBigFile && !semaphoreAcquired)
                                        {
                                            currentFileCallback?.Invoke($"⏸ Auto Pause: {Path.GetFileName(filePath)}");
                                            SaveServices.BigFileSemaphore.Wait(cancelToken);
                                            semaphoreAcquired = true;
                                        }
                                        lock (_logLock) { state.state = "Active"; logger.WriteLog(state); }
                                    }

                                    bool shouldUpdateUI = false;
                                    lock (_logLock)
                                    {
                                        if (state.state == "Paused (User)") state.state = "Active";

                                        _bytesCopied += chunkSize;
                                        if (state.totalFilesSize > 0)
                                            state.progression = (int)((_bytesCopied * 100) / state.totalFilesSize);
                                        state.sizeFileRemaining = state.totalFilesSize - _bytesCopied;
                                        state.time = DateTime.Now;

                                        if (updateTimer.ElapsedMilliseconds > 250 || _bytesCopied >= state.totalFilesSize)
                                        {
                                            logger.WriteLog(state);
                                            shouldUpdateUI = true;
                                            updateTimer.Restart();
                                        }
                                    }

                                    if (shouldUpdateUI)
                                    {
                                        progress?.Report(state.progression ?? 0);
                                        currentFileCallback?.Invoke($"Updating: {Path.GetFileName(filePath)} ({state.progression}%)");
                                    }
                                },
                                () =>
                                {
                                    lock (_logLock)
                                    {
                                        state.state = "Paused (User)";
                                        state.time = DateTime.Now;
                                        logger.WriteLog(state);
                                    }
                                    if (isBigFile && semaphoreAcquired)
                                    {
                                        SaveServices.BigFileSemaphore.Release();
                                        semaphoreAcquired = false;
                                    }
                                },
                                () =>
                                {
                                    if (isBigFile && !semaphoreAcquired)
                                    {
                                        currentFileCallback?.Invoke($"⏸ Auto Pause: {Path.GetFileName(filePath)}");
                                        SaveServices.BigFileSemaphore.Wait(cancelToken);
                                        semaphoreAcquired = true;
                                    }
                                    lock (_logLock)
                                    {
                                        state.state = "Active";
                                        state.time = DateTime.Now;
                                        logger.WriteLog(state);
                                    }
                                }, pauseEvent, cancelToken);

                                fileTimer.Stop();
                                _filesCopied++;

                                lock (_logLock)
                                {
                                    state.nbFilesLeftToDo = state.totalFilesToCopy - _filesCopied;

                                    if (updateTimer.ElapsedMilliseconds > 250 || _filesCopied == state.totalFilesToCopy)
                                    {
                                        logger.WriteLog(state);
                                        updateTimer.Restart();
                                    }

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
                            catch (OperationCanceledException)
                            {
                                return;
                            }
                            finally
                            {
                                if (semaphoreAcquired) SaveServices.BigFileSemaphore.Release();
                            }
                        }
                        else
                        {
                            _filesCopied++;
                            lock (_logLock) { state.nbFilesLeftToDo = state.totalFilesToCopy - _filesCopied; }
                        }
                    }
                    finally
                    {
                        if (isPriority)
                        {
                            Interlocked.Decrement(ref SaveServices.PendingPriorityFiles);
                            myRemainingPriorityFiles--;
                        }
                    }
                }
            }
            finally
            {
                if (myRemainingPriorityFiles > 0)
                {
                    Interlocked.Add(ref SaveServices.PendingPriorityFiles, -myRemainingPriorityFiles);
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