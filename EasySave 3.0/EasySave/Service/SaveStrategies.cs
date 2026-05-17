using EasyLog;
using EasySave.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Service
{
    public abstract class SaveStrategyBase : ISaveStrategy
    {
        protected int _filesCopied = 0;
        protected long _bytesCopied = 0;
        protected static readonly object _logLock = new object();

        protected abstract bool ShouldCopy(string sourceFile, string destFile);

        public async Task<string> SaveAsync(Backup job, string businessSoftware, string encryptedExtensions, string priorityExtensions, long maxFileSizeBytes, string logDestination, string serverUrl, string userName, ILogStrategy logger, IFormatter formatter, IProgress<int> progress, Action<string> currentFileCallback, CancellationToken cancelToken, ManualResetEventSlim pauseEvent)
        {
            LogModel state = new LogModel();
            state.name = job.Name;
            state.state = "Active";
            state.progression = 0;
            state.time = DateTime.Now;

            string appDataInit = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string logDirInit = Path.Combine(appDataInit, "EasySave", "logs");
            LogDaily dailyLogger = new LogDaily(logDirInit, formatter, serverUrl, userName);
            dailyLogger.Destination = logDestination;

            try
            {
                string source = SaveService.ConvertToUNC(job.FileSource);
                string target = SaveService.ConvertToUNC(job.FileDestination);

                if (Directory.Exists(source) == false)
                {
                    lock (_logLock)
                    {
                        state.state = "Error";
                        state.time = DateTime.Now;
                        logger.WriteLog(state);
                    }
                    return "error_source_missing";
                }

                _filesCopied = 0;
                _bytesCopied = 0;

                string[] allFiles = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
                long totalSize = 0;
                foreach (string file in allFiles)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                }

                state.totalFilesToCopy = allFiles.Length;
                state.totalFilesSize = totalSize;
                state.nbFilesLeftToDo = allFiles.Length;
                state.sizeFileRemaining = totalSize;

                await Task.Run(() => CopyFiles(source, target, allFiles, businessSoftware, encryptedExtensions, priorityExtensions, maxFileSizeBytes, state, logger, dailyLogger, progress, currentFileCallback, cancelToken, pauseEvent));

                if (cancelToken.IsCancellationRequested)
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
                    state.state = "End";
                    state.progression = 100;
                    state.nbFilesLeftToDo = 0;
                    state.sizeFileRemaining = 0;
                    state.time = DateTime.Now;
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
                return "Error: " + ex.Message;
            }
        }

        private void CopyFiles(string source, string target, string[] allFiles, string businessSoftware, string encryptedExtensions, string priorityExtensions, long maxFileSizeBytes, LogModel state, ILogStrategy logger, ILogStrategy dailyLogger, IProgress<int> progress, Action<string> currentFileCallback, CancellationToken cancelToken, ManualResetEventSlim pauseEvent)
        {
            string[] prioExts = ParseExtensions(priorityExtensions);
            string[] sortedFiles = SortFiles(allFiles, prioExts);

            int myRemainingPriorityFiles = CountPriorityFiles(sortedFiles, prioExts);
            Interlocked.Add(ref SaveService.PendingPriorityFiles, myRemainingPriorityFiles);

            Stopwatch updateTimer = Stopwatch.StartNew();

            try
            {
                foreach (string filePath in sortedFiles)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    bool isPriority = false;
                    if (Array.IndexOf(prioExts, Path.GetExtension(filePath).ToLower()) != -1)
                    {
                        isPriority = true;
                    }

                    try
                    {
                        pauseEvent.Wait(cancelToken);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }

                    if (HandleBusinessSoftwareWait(businessSoftware, cancelToken, state, logger, currentFileCallback))
                    {
                        return;
                    }

                    if (isPriority == false)
                    {
                        if (HandlePriorityWait(cancelToken, state, logger, currentFileCallback))
                        {
                            return;
                        }
                    }

                    try
                    {
                        string relativePath = filePath.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar);
                        string destPath = Path.Combine(target, relativePath);

                        if (ShouldCopy(filePath, destPath))
                        {
                            ProcessSingleFile(filePath, destPath, businessSoftware, encryptedExtensions, maxFileSizeBytes, state, logger, dailyLogger, progress, currentFileCallback, cancelToken, pauseEvent, updateTimer);
                        }
                        else
                        {
                            _filesCopied++;
                            lock (_logLock)
                            {
                                state.nbFilesLeftToDo = state.totalFilesToCopy - _filesCopied;
                            }
                        }
                    }
                    finally
                    {
                        if (isPriority)
                        {
                            Interlocked.Decrement(ref SaveService.PendingPriorityFiles);
                            myRemainingPriorityFiles--;
                        }
                    }
                }
            }
            finally
            {
                if (myRemainingPriorityFiles > 0)
                {
                    Interlocked.Add(ref SaveService.PendingPriorityFiles, -myRemainingPriorityFiles);
                }
            }
        }

        private string[] ParseExtensions(string extensionsString)
        {
            string cleanString = extensionsString;
            if (cleanString == null)
            {
                cleanString = "";
            }

            string[] rawExts = cleanString.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> parsedList = new List<string>();

            foreach (string ext in rawExts)
            {
                string trimmed = ext.Trim().ToLower();
                if (string.IsNullOrEmpty(trimmed) == false)
                {
                    if (trimmed.StartsWith(".") == false)
                    {
                        trimmed = "." + trimmed;
                    }
                    parsedList.Add(trimmed);
                }
            }

            return parsedList.ToArray();
        }

        private string[] SortFiles(string[] allFiles, string[] prioExts)
        {
            string[] sorted = new string[allFiles.Length];
            Array.Copy(allFiles, sorted, allFiles.Length);

            FileComparer comparer = new FileComparer(prioExts);
            Array.Sort(sorted, comparer);

            return sorted;
        }

        private int CountPriorityFiles(string[] sortedFiles, string[] prioExts)
        {
            int count = 0;
            foreach (string file in sortedFiles)
            {
                if (Array.IndexOf(prioExts, Path.GetExtension(file).ToLower()) != -1)
                {
                    count++;
                }
            }
            return count;
        }

        private bool HandleBusinessSoftwareWait(string businessSoftware, CancellationToken cancelToken, LogModel state, ILogStrategy logger, Action<string> currentFileCallback)
        {
            bool wasBlocked = false;
            while (BusinessSoftwareDetector.IsRunning(businessSoftware))
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return true;
                }
                if (wasBlocked == false)
                {
                    lock (_logLock)
                    {
                        state.state = "Paused (Business Software)";
                        logger.WriteLog(state);
                    }
                    if (currentFileCallback != null)
                    {
                        currentFileCallback.Invoke("⏸ Blocked by process: " + businessSoftware);
                    }
                    wasBlocked = true;
                }
                Thread.Sleep(2000);
            }
            if (wasBlocked)
            {
                lock (_logLock)
                {
                    state.state = "Active";
                }
            }
            return false;
        }

        private bool HandlePriorityWait(CancellationToken cancelToken, LogModel state, ILogStrategy logger, Action<string> currentFileCallback)
        {
            bool wasPaused = false;
            while (Interlocked.Read(ref SaveService.PendingPriorityFiles) > 0)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return true;
                }
                if (wasPaused == false)
                {
                    lock (_logLock)
                    {
                        state.state = "Paused (Priority Waiting)";
                        logger.WriteLog(state);
                    }
                    if (currentFileCallback != null)
                    {
                        currentFileCallback.Invoke("⏸ Auto Pause: Waiting for priority jobs");
                    }
                    wasPaused = true;
                }
                Thread.Sleep(1000);
            }
            if (wasPaused)
            {
                lock (_logLock)
                {
                    state.state = "Active";
                    logger.WriteLog(state);
                }
            }
            return false;
        }

        private void ProcessSingleFile(string filePath, string destPath, string businessSoftware, string encryptedExtensions, long maxFileSizeBytes, LogModel state, ILogStrategy logger, ILogStrategy dailyLogger, IProgress<int> progress, Action<string> currentFileCallback, CancellationToken cancelToken, ManualResetEventSlim pauseEvent, Stopwatch updateTimer)
        {
            string destDir = Path.GetDirectoryName(destPath);
            if (Directory.Exists(destDir) == false)
            {
                Directory.CreateDirectory(destDir);
            }

            FileInfo currentFileInfo = new FileInfo(filePath);
            long fileSize = currentFileInfo.Length;
            bool isBigFile = false;
            if (fileSize > maxFileSizeBytes)
            {
                isBigFile = true;
            }

            bool semaphoreAcquired = false;

            Stopwatch fileTimer = Stopwatch.StartNew();
            long encTime = 0;
            bool fileCopiedSuccessfully = false;

            try
            {
                if (isBigFile)
                {
                    if (SaveService.BigFileSemaphore.Wait(0) == false)
                    {
                        lock (_logLock)
                        {
                            state.state = "Paused (Big File Limit)";
                            logger.WriteLog(state);
                        }
                        if (currentFileCallback != null)
                        {
                            currentFileCallback.Invoke("⏸ Auto Pause: " + Path.GetFileName(filePath));
                        }

                        SaveService.BigFileSemaphore.Wait(cancelToken);

                        lock (_logLock)
                        {
                            state.state = "Active";
                        }
                    }
                    semaphoreAcquired = true;
                }

                encTime = SaveService.CopyOrEncrypt(filePath, destPath, encryptedExtensions, (chunkSize) =>
                {
                    bool wasPausedBySoftware = false;
                    while (BusinessSoftwareDetector.IsRunning(businessSoftware))
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            return;
                        }
                        if (wasPausedBySoftware == false)
                        {
                            lock (_logLock)
                            {
                                state.state = "Paused (Business Software)";
                                logger.WriteLog(state);
                            }
                            if (currentFileCallback != null)
                            {
                                currentFileCallback.Invoke("⏸ Blocked by process: " + businessSoftware);
                            }
                            wasPausedBySoftware = true;

                            if (isBigFile && semaphoreAcquired)
                            {
                                SaveService.BigFileSemaphore.Release();
                                semaphoreAcquired = false;
                            }
                        }
                        Thread.Sleep(1000);
                    }
                    if (wasPausedBySoftware)
                    {
                        if (isBigFile && semaphoreAcquired == false)
                        {
                            if (currentFileCallback != null)
                            {
                                currentFileCallback.Invoke("⏸ Auto Pause: " + Path.GetFileName(filePath));
                            }
                            SaveService.BigFileSemaphore.Wait(cancelToken);
                            semaphoreAcquired = true;
                        }
                        lock (_logLock)
                        {
                            state.state = "Active";
                            logger.WriteLog(state);
                        }
                    }

                    UpdateProgressAndLogs(chunkSize, state, logger, progress, currentFileCallback, updateTimer, filePath);
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
                        SaveService.BigFileSemaphore.Release();
                        semaphoreAcquired = false;
                    }
                },
                () =>
                {
                    if (isBigFile && semaphoreAcquired == false)
                    {
                        if (currentFileCallback != null)
                        {
                            currentFileCallback.Invoke("⏸ Auto Pause: " + Path.GetFileName(filePath));
                        }
                        SaveService.BigFileSemaphore.Wait(cancelToken);
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
                fileCopiedSuccessfully = true;
                _filesCopied++;

                lock (_logLock)
                {
                    state.nbFilesLeftToDo = state.totalFilesToCopy - _filesCopied;

                    if (updateTimer.ElapsedMilliseconds > 250 || _filesCopied == state.totalFilesToCopy)
                    {
                        logger.WriteLog(state);
                        updateTimer.Restart();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                if (semaphoreAcquired)
                {
                    SaveService.BigFileSemaphore.Release();
                }

                if (fileCopiedSuccessfully)
                {
                    fileTimer.Stop();
                    try
                    {
                        LogModel dailyLog = new LogModel();
                        dailyLog.name = state.name;
                        dailyLog.fileSource = filePath;
                        dailyLog.fileDestination = destPath;
                        dailyLog.fileSize = fileSize;
                        dailyLog.fileTransferTime = fileTimer.Elapsed.TotalMilliseconds;
                        dailyLog.encryptionTime = encTime;
                        dailyLog.time = DateTime.Now;
                        dailyLogger.WriteLog(dailyLog);
                    }
                    catch (Exception logEx)
                    {
                        System.Diagnostics.Debug.WriteLine("[DailyLog] Write failed: " + logEx.Message);
                        if (currentFileCallback != null)
                        {
                            currentFileCallback.Invoke("[DailyLog ERR] " + logEx.Message);
                        }
                    }
                }
            }
        }

        private void UpdateProgressAndLogs(int chunkSize, LogModel state, ILogStrategy logger, IProgress<int> progress, Action<string> currentFileCallback, Stopwatch updateTimer, string filePath)
        {
            bool shouldUpdateUI = false;
            lock (_logLock)
            {
                if (state.state == "Paused (User)")
                {
                    state.state = "Active";
                }

                _bytesCopied += chunkSize;
                if (state.totalFilesSize > 0)
                {
                    state.progression = (int)((_bytesCopied * 100) / state.totalFilesSize);
                }
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
                if (progress != null)
                {
                    int currentProgression = 0;
                    if (state.progression.HasValue)
                    {
                        currentProgression = state.progression.Value;
                    }
                    progress.Report(currentProgression);
                }
                if (currentFileCallback != null)
                {
                    currentFileCallback.Invoke("Updating: " + Path.GetFileName(filePath) + " (" + state.progression + "%)");
                }
            }
        }

        private class FileComparer : IComparer<string>
        {
            private string[] _prioExts;

            public FileComparer(string[] prioExts)
            {
                _prioExts = prioExts;
            }

            public int Compare(string x, string y)
            {
                string extX = Path.GetExtension(x).ToLower();
                string extY = Path.GetExtension(y).ToLower();

                int indexX = Array.IndexOf(_prioExts, extX);
                int indexY = Array.IndexOf(_prioExts, extY);

                if (indexX == -1)
                {
                    indexX = int.MaxValue;
                }
                if (indexY == -1)
                {
                    indexY = int.MaxValue;
                }

                if (indexX != indexY)
                {
                    return indexX.CompareTo(indexY);
                }

                long lenX = new FileInfo(x).Length;
                long lenY = new FileInfo(y).Length;

                return lenX.CompareTo(lenY);
            }
        }
    }

    public class SaveComplete : SaveStrategyBase
    {
        protected override bool ShouldCopy(string sourceFile, string destFile)
        {
            return true;
        }
    }

    public class SaveDifferential : SaveStrategyBase
    {
        protected override bool ShouldCopy(string sourceFile, string destFile)
        {
            if (File.Exists(destFile) == false)
            {
                return true;
            }

            DateTime sourceTime = File.GetLastWriteTime(sourceFile);
            DateTime destTime = File.GetLastWriteTime(destFile);

            if (sourceTime > destTime)
            {
                return true;
            }

            return false;
        }
    }
}