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
        private long _totalEncryptionTime = 0;
        private static readonly object _logLock = new object();

        public async Task<string> SaveAsync(Backup job, string businessSoftware, string encryptedExtensions, ILogStrategy logger, IFormatter formatter, IProgress<int> progress, Action<string> currentFileCallback, CancellationToken cancelToken, ManualResetEventSlim pauseEvent)
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
                long totalSize = allFiles.Sum(f => new FileInfo(f).Length);

                LogModel state = new LogModel
                {
                    name = job.Name,
                    state = "Active",
                    totalFilesToCopy = allFiles.Length,
                    totalFilesSize = totalSize,
                    nbFilesLeftToDo = allFiles.Length,
                    sizeFileRemaining = totalSize,
                    progression = 0,
                    time = DateTime.Now
                };

                Stopwatch timer = Stopwatch.StartNew();
                string recursiveError = await Task.Run(() => CopyDirectoryRecursive(source, target, businessSoftware, encryptedExtensions, state, logger, dailyLogger, progress, currentFileCallback, cancelToken, pauseEvent));
                timer.Stop();

                if (recursiveError != null) return recursiveError;

                lock (_logLock)
                {
                    state.state = "End";
                    state.progression = 100;
                    state.nbFilesLeftToDo = 0;
                    state.sizeFileRemaining = 0;
                    state.time = DateTime.Now;
                    state.currentSourceFile = null;
                    state.currentDestinationFile = null;
                    logger.WriteLog(state);
                }

                progress?.Report(100);
                currentFileCallback?.Invoke("Finished.");

                return null;
            }
            catch (OperationCanceledException)
            {
                return "Job stopped.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private string CopyDirectoryRecursive(string src, string trg, string businessSoftware, string encryptedExtensions, LogModel state, ILogStrategy logger, ILogStrategy dailyLogger, IProgress<int> progress, Action<string> currentFileCallback, CancellationToken cancelToken, ManualResetEventSlim pauseEvent)
        {
            if (!Directory.Exists(trg)) Directory.CreateDirectory(trg);

            foreach (string filePath in Directory.GetFiles(src))
            {
                cancelToken.ThrowIfCancellationRequested();
                pauseEvent.Wait(cancelToken);

                while (BusinessSoftwareDetector.IsRunning(businessSoftware))
                {
                    cancelToken.ThrowIfCancellationRequested();
                    lock (_logLock)
                    {
                        state.state = "Paused (Business Software)";
                        state.time = DateTime.Now;
                        logger.WriteLog(state);
                    }
                    Thread.Sleep(2000);
                }

                lock (_logLock)
                {
                    state.state = "Active";
                }

                string destPath = Path.Combine(trg, Path.GetFileName(filePath));

                if (ShouldCopy(filePath, destPath))
                {
                    long fileSize = new FileInfo(filePath).Length;
                    bool isBigFile = fileSize > SaveServices.BIG_FILE_THRESHOLD;

                    lock (_logLock)
                    {
                        state.currentSourceFile = filePath;
                        state.currentDestinationFile = destPath;
                    }

                    try
                    {
                        if (isBigFile)
                        {
                            if (SaveServices.BigFileSemaphore.CurrentCount == 0)
                            {
                                currentFileCallback?.Invoke($"⏳ Waiting for bandwidth: {Path.GetFileName(filePath)} (>50MB)");
                            }
                            SaveServices.BigFileSemaphore.Wait(cancelToken);
                        }

                        Stopwatch fileTimer = Stopwatch.StartNew();

                        long encTime = SaveServices.CopyOrEncrypt(filePath, destPath, encryptedExtensions, (chunkSize) =>
                        {
                            lock (_logLock)
                            {
                                _bytesCopied += chunkSize;
                                if (state.totalFilesSize > 0)
                                    state.progression = (int)((_bytesCopied * 100) / state.totalFilesSize);

                                state.sizeFileRemaining = state.totalFilesSize - _bytesCopied;
                                state.time = DateTime.Now;
                            }
                            progress?.Report(state.progression ?? 0);
                            currentFileCallback?.Invoke($"Copying: {Path.GetFileName(filePath)} ({state.progression ?? 0}%)");
                        }, pauseEvent, cancelToken);

                        fileTimer.Stop();

                        _filesCopied++;
                        _totalEncryptionTime += encTime;

                        lock (_logLock)
                        {
                            state.nbFilesLeftToDo = state.totalFilesToCopy - _filesCopied;
                            state.time = DateTime.Now;
                            logger.WriteLog(state);

                            LogModel dailyLog = new LogModel
                            {
                                name = state.name,
                                fileSource = filePath,
                                fileDestination = destPath,
                                fileSize = fileSize,
                                fileTransferTime = fileTimer.Elapsed.TotalMilliseconds,
                                encryptionTime = encTime,
                                time = DateTime.Now
                            };
                            dailyLogger.WriteLog(dailyLog);
                        }
                    }
                    finally
                    {
                        if (isBigFile) SaveServices.BigFileSemaphore.Release();
                    }
                }
                else
                {
                    _filesCopied++;
                    lock (_logLock)
                    {
                        state.nbFilesLeftToDo = state.totalFilesToCopy - _filesCopied;
                        state.time = DateTime.Now;
                        logger.WriteLog(state);
                    }
                }
            }

            foreach (string dirPath in Directory.GetDirectories(src))
            {
                string error = CopyDirectoryRecursive(dirPath, Path.Combine(trg, Path.GetFileName(dirPath)), businessSoftware, encryptedExtensions, state, logger, dailyLogger, progress, currentFileCallback, cancelToken, pauseEvent);
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