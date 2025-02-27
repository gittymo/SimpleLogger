using System.Text;

namespace SimpleLogger
{
    public sealed class SimpleLogger
    {
        public SimpleLogger(string logFilePath, int maximumRollFilesToKeep = 0)
        {
            LogFilePath = logFilePath;
            RetentionPeriod = TimeSpan.MinValue;
            MaximumLogFileSizeInBytes = -1;
            MaximumRollFilesToKeep = maximumRollFilesToKeep;
        }

        public SimpleLogger(string logFilePath, string retentionPeriodString, int maximumRollFilesToKeep = 0)
        {
            LogFilePath = logFilePath;
            RetentionPeriod = TimeSpan.Parse(retentionPeriodString);
            MaximumLogFileSizeInBytes = -1;
            MaximumRollFilesToKeep = maximumRollFilesToKeep;
        }

        public SimpleLogger(string logFilePath, int maximumLogFileSizeInBytes, int maximumRollFilesToKeep = 0)
        {
            LogFilePath = logFilePath;
            RetentionPeriod = TimeSpan.MinValue;
            MaximumLogFileSizeInBytes = maximumLogFileSizeInBytes;
            MaximumRollFilesToKeep = maximumRollFilesToKeep;
        }

        // Write a formatted log message to the log filepath, replacing {0}, {1}, etc. with the provided arguments
        public void Log(string level, string message, params object[] args)
        {
            // Check to see if the log file is older than the retention period or if the log file is too large.
            if (File.Exists(LogFilePath))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(LogFilePath);
                TimeSpan logFileAge = DateTime.Now - lastWriteTime;
                FileInfo logFileInfo = new FileInfo(LogFilePath);

                if ((RetentionPeriod > TimeSpan.MinValue && logFileAge > RetentionPeriod) || (MaximumLogFileSizeInBytes > 0 && logFileInfo.Length > MaximumLogFileSizeInBytes))
                {
                    // If we have enabled rolling, roll the log file.
                    if (MaximumRollFilesToKeep > 0)
                    {
                        // Create a roll file with the current date and time appended to the log file name.
                        string rollFilePath = Path.Combine(Path.GetDirectoryName(LogFilePath), Path.GetFileNameWithoutExtension(LogFilePath) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(LogFilePath));
                        File.Move(LogFilePath, rollFilePath);

                        // Get a list of all rolled log files with the same name as the current log file, sorted by last write time.
                        string[] rolledLogFiles = Directory.GetFiles(Path.GetDirectoryName(LogFilePath), Path.GetFileNameWithoutExtension(LogFilePath) + "_*")
                            .OrderByDescending(f => File.GetLastWriteTime(f))
                            .ToArray();
                        // Delete any rolled file whose index is greater than MaximumRollFilesToKeep
                        for (int i = MaximumRollFilesToKeep; i < rolledLogFiles.Length; i++)
                        {
                            File.Delete(rolledLogFiles[i]);
                        }
                    }
                }
            }

            StringBuilder sb = new StringBuilder(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss - "));
            sb.Append(level);
            sb.Append(": ");
            sb.AppendFormat(message, args);
            sb.Append(Environment.NewLine);
            File.AppendAllText(LogFilePath, sb.ToString());
        }

        public void LogWarn(string message, params object[] args)
        {
            Log("WARNING", message, args);
        }

        public void LogError(string message, params object[] args)
        {
            Log("ERROR", message, args);
        }

        public void LogInfo(string message, params object[] args)
        {
            Log("INFO", message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            Log("DEBUG", message, args);
        }

        public string LogFilePath { get; init; }
        public TimeSpan RetentionPeriod { get; init; }
        public int MaximumLogFileSizeInBytes { get; init; }
        public int MaximumRollFilesToKeep { get; init; }
    }
}
