using System.Diagnostics;
using System.Linq;

namespace EasySave.Services
{
    public static class BusinessSoftwareDetector
    {
        public static bool IsRunning(string businessSoftware)
        {
            if (string.IsNullOrWhiteSpace(businessSoftware)) return false;

            string[] softwareToBlock = businessSoftware.Split(';');

            Process[] runningProcesses = Process.GetProcesses();

            foreach (string software in softwareToBlock)
            {
                string cleanName = software.Trim().ToLower();
                if (string.IsNullOrEmpty(cleanName)) continue;

                if (runningProcesses.Any(p => p.ProcessName.ToLower().Contains(cleanName)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}