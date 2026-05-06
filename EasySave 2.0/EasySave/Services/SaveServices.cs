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

        public static long CopyOrEncrypt(string sourceFile, string destinationFile)
        {
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
                Arguments = $"source \"{source}\" destination \"{destination}\"",
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
                throw new Exception("Encryption failed, backup aborted: " + ex.Message);
            }
        }

        public static string ConvertToUNC(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}