namespace PCSleepWatcher
{
    public static class Logger
    {
        private static readonly int MaxLogEntries = 100;

        public static void LogInformation(string message)
        {
            var logItem = new LogItem { Message = message, Type = LogItemType.Information };
            PrintLog(logItem);
        }

        public static void LogError(string message)
        {
            var logItem = new LogItem { Message = message, Type = LogItemType.Error };
            PrintLog(logItem);
        }

        public static void LogWarning(string message)
        {
            var logItem = new LogItem { Message = message, Type = LogItemType.Warning };
            PrintLog(logItem);
        }

        public static void LogSuccess(string message)
        {
            var logItem = new LogItem { Message = message, Type = LogItemType.Success };
            PrintLog(logItem);
        }

        private static void PrintLog(LogItem item)
        {
            item.Time = DateTime.Now;
            var logFolderPath = Path.Combine(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    AppDomain.CurrentDomain.RelativeSearchPath ?? ""), "Log");
            Directory.CreateDirectory(logFolderPath);
            var logFilePath = Path.Combine(
                logFolderPath,
                $"log_{DateTime.Now:dd_MM_yyyy}.log");
            File.AppendAllText(logFilePath, item.FormattedMessage + Environment.NewLine);
        }
    }

    public class LogItem
    {
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime Time { get; set; }

        public string FormattedMessage => $"{Time:dd.MM.yyyy HH:mm:ss} | {Type.ToUpperInvariant(),-11} | {Message}";
    }

    public static class LogItemType
    {
        public static readonly string Information = "Information";
        public static readonly string Error = "Error";
        public static readonly string Warning = "Warning";
        public static readonly string Success = "Success";

        public static string GetUpperAndEqualLength(string value)
        {
            return value.ToUpperInvariant().PadRight(11);
        }
    }
}