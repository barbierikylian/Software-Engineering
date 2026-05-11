using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EasySave.Services
{
    public static class SaveServices
    {
        public static SemaphoreSlim BigFileSemaphore = new SemaphoreSlim(1, 1);
        public const long BIG_FILE_THRESHOLD = 50 * 1024 * 1024;

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

        public static long CopyOrEncrypt(string sourceFile, string destinationFile, string encryptedExtensions)
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

            File.Copy(sourceFile, destinationFile, true);

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

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        public static string ConvertToUNC(string path) => Path.GetFullPath(path);
    }
}