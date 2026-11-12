using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Plugins.Core
{
    public static class PluginLogger
    {
        private static string _logPath;
        private static readonly object _lock = new object();
        private static bool _initialized;

        public static void Initialize(string profilesFolder, string profileName)
        {
            lock (_lock)
            {
                if (!Directory.Exists(profilesFolder))
                {
                    try { Directory.CreateDirectory(profilesFolder); }
                    catch { return; }
                }
                
                _logPath = Path.Combine(profilesFolder, $"HTM_ATC_{profileName}.log");
                _initialized = true;
                
                Log("=== HTM_ATC Plugin Started ===");
            }
        }

        public static void Log(string message)
        {
            if (!_initialized || string.IsNullOrEmpty(_logPath))
                return;

            lock (_lock)
            {
                try
                {
                    int threadId = Thread.CurrentThread.ManagedThreadId;
                    string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    string line = $"[{timestamp}] [T{threadId}] {message}{Environment.NewLine}";
                    File.AppendAllText(_logPath, line, Encoding.UTF8);
                }
                catch
                {
                }
            }
        }

        public static void LogException(string context, Exception ex)
        {
            Log($"EXCEPTION in {context}: {ex.GetType().Name}: {ex.Message}");
        }

        public static void LogWizardFinish()
        {
            Log("Setup Wizard finished - config saved");
        }

        public static void LogConfigSave()
        {
            Log("Configuration saved to disk");
        }

        public static void LogFormOpen(string formName)
        {
            Log($"Form opened: {formName}");
        }

        public static void LogFormClose(string formName)
        {
            Log($"Form closed: {formName}");
        }

        public static void LogLoopException(Exception ex)
        {
            Log($"Loop exception: {ex.GetType().Name}: {ex.Message}");
        }

        public static void LogToolChange(string message)
        {
            Log($"[TC] {message}");
        }
    }
}
