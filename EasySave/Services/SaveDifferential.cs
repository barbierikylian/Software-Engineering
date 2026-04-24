using System;
using System.IO;
using System.Diagnostics;
using EasySave.Model;
using EasyLog;

namespace EasySave.Services
{
    public class SaveDifferential : ISaveStrategy
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

                Console.Title = "EasySave - Finished";
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($" Job: {job.Name}");
                Console.WriteLine($" Status: Success");
                Console.WriteLine($" Files: {_filesCopied}");
                Console.WriteLine($" Time: {timer.ElapsedMilliseconds} ms");
                Console.WriteLine("--------------------------------------------------");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");
            }
        }

        private void CopyDirectoryRecursive(string src, string trg, LogModel state, ILogStrategy logger)
        {
            if (!Directory.Exists(trg)) Directory.CreateDirectory(trg);

            foreach (string filePath in Directory.GetFiles(src))
            {
                string destPath = Path.Combine(trg, Path.GetFileName(filePath));

                if (ShouldCopy(filePath, destPath))
                {
                    bool isNewFile = !File.Exists(destPath);
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

                    Console.Title = $"[{state.progression}%] EasySave - Copying: {state.name}";

                    string fileName = Path.GetFileName(filePath);
                    if (isNewFile)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("[+] ");
                        Console.ResetColor();
                        Console.WriteLine($"Added   : {fileName} ({state.progression}%)");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("[~] ");
                        Console.ResetColor();
                        Console.WriteLine($"Updated : {fileName} ({state.progression}%)");
                    }
                }
            }

            foreach (string dirPath in Directory.GetDirectories(src))
            {
                CopyDirectoryRecursive(dirPath, Path.Combine(trg, Path.GetFileName(dirPath)), state, logger);
            }
        }

        private bool ShouldCopy(string sourceFile, string destFile)
        {
            if (!File.Exists(destFile)) return true;
            return File.GetLastWriteTime(sourceFile) > File.GetLastWriteTime(destFile);
        }
    }
}