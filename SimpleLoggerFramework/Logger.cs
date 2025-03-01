using System.Text;
using System.IO;
using System.Linq;
using System;

namespace SimpleLogger
{
    public sealed class Logger
    {
        public Logger(string logFilePath)
        {
            // Make sure the log file path is exists.
            if (!String.IsNullOrWhiteSpace(logFilePath) && !Directory.Exists(Path.GetDirectoryName(logFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            }

            _logFilePath = logFilePath;
            _retentionPeriod = TimeSpan.MinValue;
            _maximumLogFileSizeInBytes = -1;
            _maximumRollFilesToKeep = 0;
        }

        public Logger(string logFilePath, string retentionPeriodString, int maximumRollFilesToKeep = 0)
        {
            // Make sure the log file path is exists.
            if (!String.IsNullOrWhiteSpace(logFilePath) && !Directory.Exists(Path.GetDirectoryName(logFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            }
            _logFilePath = logFilePath;
            _retentionPeriod = TimeSpan.Parse(retentionPeriodString);
            _maximumLogFileSizeInBytes = -1;
            _maximumRollFilesToKeep = maximumRollFilesToKeep;
        }

        public Logger(string logFilePath, int maximumLogFileSizeInBytes, int maximumRollFilesToKeep = 0)
        {
            // Make sure the log file path is exists.
            if (!String.IsNullOrWhiteSpace(logFilePath) && !Directory.Exists(Path.GetDirectoryName(logFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            }
            _logFilePath = logFilePath;
            _retentionPeriod = TimeSpan.MinValue;
            _maximumLogFileSizeInBytes = maximumLogFileSizeInBytes;
            _maximumRollFilesToKeep = maximumRollFilesToKeep;
        }

        // Write a formatted log message to the log filepath, replacing {0}, {1}, etc. with the provided arguments
        public void Log(string level, string message, params object[] args)
        {
            // Lock the log file to prevent multiple threads from writing to it at the same time.
            lock (_lockObject)
            {
                try
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

                    StringBuilder sb = new StringBuilder(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    sb.Append(" - ");
                    sb.Append(level);
                    sb.Append(": ");
                    sb.AppendFormat(message, args);
                    sb.Append(Environment.NewLine);
                    File.AppendAllText(LogFilePath, sb.ToString());
                }
                catch (Exception)
                {
                    // Don't do anything. We don't want to throw an exception if logging fails.
                }
            }
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

        public string LogFilePath => _logFilePath;
        public TimeSpan RetentionPeriod => _retentionPeriod;
        public int MaximumLogFileSizeInBytes => _maximumLogFileSizeInBytes;
        public int MaximumRollFilesToKeep => _maximumRollFilesToKeep;

        private readonly string _logFilePath;
        private readonly TimeSpan _retentionPeriod;
        private readonly int _maximumLogFileSizeInBytes;
        private readonly int _maximumRollFilesToKeep;

        private readonly object _lockObject = new object();
    }
}
