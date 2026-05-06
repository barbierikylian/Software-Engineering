using System.Diagnostics;

namespace EasySave.Services
{
    public static class BusinessSoftwareDetector
    {
        public static bool IsRunning(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName)) return false;

            string cleanName = processName.ToLower().Replace(".exe", "");

            Process[] processes = Process.GetProcessesByName(cleanName);
            return processes.Length > 0;
        }
    }
}