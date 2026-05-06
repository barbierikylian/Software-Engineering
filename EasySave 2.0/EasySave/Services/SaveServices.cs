using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace EasySave.Services
{
    public static class SaveServices
    {
        private static readonly string[] EncryptedExtensions = { ".txt", ".json", ".xml" };

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

        private static bool IsBusinessSoftwareRunning(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName)) return false;
            string name = processName.EndsWith(".exe") ? Path.GetFileNameWithoutExtension(processName) : processName;
            return Process.GetProcessesByName(name).Length > 0;
        }

        public static long CopyOrEncrypt(string sourceFile, string destinationFile, string businessSoftware, Action<string> logInterruption)
        {
            if (IsBusinessSoftwareRunning(businessSoftware))
            {
                logInterruption?.Invoke(businessSoftware);
                throw new OperationCanceledException("BUSINESS_SOFT_DETECTED");
            }

            string destDirectory = Path.GetDirectoryName(destinationFile);
            if (!Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }

            string extension = Path.GetExtension(sourceFile).ToLower();

            if (Array.Exists(EncryptedExtensions, ext => ext == extension))
            {
                return LaunchCryptoSoft(sourceFile, destinationFile);
            }
            else
            {
                File.Copy(sourceFile, destinationFile, true);
                return 0;
            }
        }

        private static long LaunchCryptoSoft(string source, string destination)
        {
            string cryptoPath = GetCryptoSoftPath();

            if (string.IsNullOrEmpty(cryptoPath) || !File.Exists(cryptoPath))
            {
                throw new FileNotFoundException($"FATAL ERROR: CryptoSoft.exe not found. Path: {cryptoPath}");
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = cryptoPath,
                Arguments = $"\"{source}\" \"{destination}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    if (process == null) throw new Exception("Encryption engine failed to start.");

                    process.WaitForExit();

                    if (process.ExitCode < 0)
                    {
                        throw new Exception($"CryptoSoft internal error (Code: {process.ExitCode})");
                    }

                    return process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Encryption failed: " + ex.Message);
            }
        }

        public static string ConvertToUNC(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}