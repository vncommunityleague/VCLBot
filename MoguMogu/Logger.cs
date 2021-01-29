using System;
using System.IO;
using System.Threading;

namespace MoguMogu
{
    public class Logger
    {
        private static readonly object obj = new object();

        public static void Log(object msg, LogLevel level = LogLevel.Info, string m = null)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                lock (obj)
                {
                    var logText = $"[{DateTime.Now}] [{level} - {m ?? "unkown"}] {msg}";
                    Console.ForegroundColor = level switch
                    {
                        LogLevel.Warning => ConsoleColor.Yellow,
                        LogLevel.Error => ConsoleColor.Red,
                        _ => Console.ForegroundColor
                    };
                    Console.WriteLine(logText);
                    Console.ResetColor();
                    var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
                    var logFile = Path.Combine(logDir, $"{DateTime.Now:yyyy-MM-dd}.txt");
                    if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                    if (!File.Exists(logFile)) File.Create(logFile).Dispose();
                    File.AppendAllText(logFile, logText + "\n");
                }
            });
        }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}