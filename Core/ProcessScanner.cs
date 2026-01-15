using System.Diagnostics;
using System.Linq;

namespace AntiCheat.Core
{
    public static class ProcessScanner
    {
        public static bool IsMtaRunning()
        {
            return Process.GetProcesses().Any(p =>
                p.ProcessName.ToLower().Contains("gta_sa") ||
                p.ProcessName.ToLower().Contains("multitheftauto"));
        }
    }
}
