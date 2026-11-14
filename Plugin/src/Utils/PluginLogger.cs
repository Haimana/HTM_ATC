using System;
using System.IO;

namespace HTM_PRO.Utils
{
    /// <summary>
    /// Thread-safe file logger for the HTM_PRO plugin.
    /// Writes diagnostic and configuration events to the active UCCNC Profiles folder.
    /// </summary>
    public static class PluginLogger
    {
        private static readonly object _lock = new object();
        private static string _logFilePath;

        static PluginLogger()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                _logFilePath = Path.Combine(baseDir, "HTM_PRO_init.log");
            }
            catch { }
        }

        public static string LogFilePath => _logFilePath;

        public static void Initialize(string profilesDirectory, string profileName)
        {
            try
            {
                if (!Directory.Exists(profilesDirectory))
                {
                    Directory.CreateDirectory(profilesDirectory);
                }

                string safeProfile = string.IsNullOrWhiteSpace(profileName) ? "Default" : profileName.Trim();
                _logFilePath = Path.Combine(profilesDirectory, $"HTM_PRO_{safeProfile}.log");
                Log($"=== {PluginVersion.FullTitle} Logger Initialized for profile [{safeProfile}] ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HTM_PRO Logger Init Error] {ex.Message}");
            }
        }

        public static void Log(string message)
        {
            if (string.IsNullOrEmpty(_logFilePath)) return;

            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_logFilePath, line + Environment.NewLine);
                }
                catch
                {
                    // Avoid disrupting UCCNC realtime loops if disk write is contended
                }
            }
        }

        public static void LogException(string context, Exception ex)
        {
            Log($"[ERROR] {context}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }
    }
}
