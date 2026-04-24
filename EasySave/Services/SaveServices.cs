using System;
using System.IO;
using EasySave.Model;

namespace EasySave.Services
{
    // Static helpers shared by the save strategies.
    public static class SaveServices
    {
        // Copies a file, creating the destination folder if needed.
        public static void CopyFile(string sourceFile, string destinationFile)
        {
            string destDirectory = Path.GetDirectoryName(destinationFile);

            if (!Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }

            File.Copy(sourceFile, destinationFile, true);
        }

        // Normalizes a path to its absolute form (does NOT build a real UNC path).
        public static string ConvertToUNC(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}