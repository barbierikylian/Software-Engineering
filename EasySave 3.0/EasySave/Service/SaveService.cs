using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EasySave.Service
{
    public static class SaveService
    {
        public static SemaphoreSlim BigFileSemaphore = new SemaphoreSlim(1, 1);
        public const long BIG_FILE_THRESHOLD = 50 * 1024 * 1024;
        public static long PendingPriorityFiles = 0;

        private static string GetCryptoSoftPath()
        {
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo directory = new DirectoryInfo(currentPath);
            while (directory != null && !directory.GetDirectories("CryptoSoft").Any())
            {
                directory = directory.Parent;
            }
            if (directory == null) return null;
            return Path.Combine(directory.FullName, "CryptoSoft", "CryptoSoft.exe");
        }

        public static long CopyOrEncrypt(string sourceFile, string destinationFile, string encryptedExtensions, Action<int> onChunkCopied = null, Action onPause = null, Action onResume = null, ManualResetEventSlim pauseEvent = null, CancellationToken cancelToken = default)
        {
            string[] extensions = (encryptedExtensions ?? "")
                .Split(';')
                .Select(e => e.Trim().ToLower())
                .Where(e => !string.IsNullOrEmpty(e))
                .Select(e => e.StartsWith(".") ? e : "." + e)
                .ToArray();

            string destDirectory = Path.GetDirectoryName(destinationFile);
            if (!Directory.Exists(destDirectory)) Directory.CreateDirectory(destDirectory);

            string extension = Path.GetExtension(sourceFile).ToLower();

            int bufferSize = 1024 * 1024;
            byte[] buffer = new byte[bufferSize];

            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream destStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write))
            {
                int bytesRead;
                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    if (pauseEvent != null && !pauseEvent.IsSet)
                    {
                        onPause?.Invoke();
                        pauseEvent.Wait(cancelToken);
                        onResume?.Invoke();
                    }
                    else
                    {
                        pauseEvent?.Wait(cancelToken);
                    }

                    destStream.Write(buffer, 0, bytesRead);
                    onChunkCopied?.Invoke(bytesRead);
                }
            }

            if (extensions.Contains(extension))
            {
                return LaunchCryptoSoft(destinationFile, destinationFile);
            }

            return 0;
        }

        private static long LaunchCryptoSoft(string source, string destination)
        {
            string cryptoPath = GetCryptoSoftPath();
            if (string.IsNullOrEmpty(cryptoPath) || !File.Exists(cryptoPath))
            {
                throw new FileNotFoundException($"FATAL ERROR: CryptoSoft.exe not found.");
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = cryptoPath,
                Arguments = $"\"{source}\" \"{destination}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            while (true)
            {
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();

                    if (process.ExitCode == -99)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    if (process.ExitCode < 0)
                    {
                        throw new Exception($"CryptoSoft internal error (Code: {process.ExitCode})");
                    }

                    return process.ExitCode;
                }
            }
        }

        public static string ConvertToUNC(string path) => Path.GetFullPath(path);
    }
}