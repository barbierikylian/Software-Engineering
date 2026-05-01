using System;
using System.IO;
using EasySave.Model;

namespace EasySave.Services
{
    public static class SaveServices
    {
        public static void CopyFile(string sourceFile, string destinationFile)
        {
            string destDirectory = Path.GetDirectoryName(destinationFile);

            if (!Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }

            File.Copy(sourceFile, destinationFile, true);

        }

        public static string ConvertToUNC(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}