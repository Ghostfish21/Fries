# define CDFIX_DEBUG

using System;
using System.IO;

namespace Fries.HelperClassCdf {
    public static class FileLogger {
        private const string VERSION = "1.0";
        
        public static void resetLog(string assemblyName) {
# if CDFIX_DEBUG
            try {
                string tempDir = Path.GetTempPath();
                string logFilePath = Path.Combine(tempDir, $"{assemblyName}-HelperClassCdf-{VERSION}-Debug.txt");
                File.WriteAllText(logFilePath, "");
            }
            catch { }
# endif
        }
        public static void log(string message) {
# if CDFIX_DEBUG
            try {
                string tempDir = Path.GetTempPath();
                string logFilePath = Path.Combine(tempDir, $"HelperClassCdf-{VERSION}-Debug.txt");
                File.AppendAllText(logFilePath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            } catch {}
# endif
        }
        public static void log(string assemblyName, string message) {
# if CDFIX_DEBUG
            try {
                string tempDir = Path.GetTempPath();
                string logFilePath = Path.Combine(tempDir, $"{assemblyName}-HelperClassCdf-{VERSION}-Debug.txt");
                File.AppendAllText(logFilePath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            } catch {}
# endif
        }
    }
}