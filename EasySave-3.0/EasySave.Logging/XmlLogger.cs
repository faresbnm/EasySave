// Updated XmlLogger.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace EasySave.Logging
{
    public class XmlLogger : ILogger
    {
        private readonly string _logDirectory;
        public LogFormat Format { get; set; } = LogFormat.Xml;

        public XmlLogger()
        {
            _logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EasySave",
                "Logs");

            Directory.CreateDirectory(_logDirectory);
        }

        public void LogTransfer(string backupName, string sourcePath, string targetPath, long fileSize, double transferTimeMs, double encryptionTimeMs)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                BackupName = backupName,
                SourcePath = sourcePath,
                TargetPath = targetPath,
                FileSize = fileSize,
                TransferTimeMs = transferTimeMs,
                Status = transferTimeMs >= 0 ? "SUCCESS" : "FAILED",
                EncryptionTimeMs = encryptionTimeMs,
                EncryptionStatus = encryptionTimeMs > 0 ? "ENCRYPTED" :
                         encryptionTimeMs < 0 ? "ENCRYPTION_FAILED" : "NOT_ENCRYPTED"
            };

            string logFile = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.xml");

            try
            {
                lock (typeof(XmlLogger)) // Global lock for all XmlLogger instances
                {
                    List<LogEntry> entries = new List<LogEntry>();

                    // Read existing entries if file exists
                    if (File.Exists(logFile))
                    {
                        try
                        {
                            var deserializer = new XmlSerializer(typeof(LogEntries));
                            using (var reader = new StreamReader(logFile))
                            {
                                var existingEntries = (LogEntries)deserializer.Deserialize(reader);
                                if (existingEntries != null && existingEntries.Entries != null)
                                {
                                    entries.AddRange(existingEntries.Entries);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            entries = new List<LogEntry>();
                        }
                    }

                    // Add new entry
                    entries.Add(logEntry);

                    // Write all entries back to file
                    var logEntries = new LogEntries { Entries = entries };
                    var settings = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = "  ",
                        NewLineChars = "\n",
                        NewLineHandling = NewLineHandling.Replace
                    };

                    var serializer = new XmlSerializer(typeof(LogEntries));
                    using (var writer = XmlWriter.Create(logFile, settings))
                    {
                        serializer.Serialize(writer, logEntries);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }
    }

    [XmlRoot("LogEntries")]
    public class LogEntries
    {
        [XmlElement("LogEntry")]
        public List<LogEntry> Entries { get; set; } = new List<LogEntry>();
    }

    public class LogEntry
    {
        public string Timestamp { get; set; }
        public string BackupName { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public long FileSize { get; set; }
        public double TransferTimeMs { get; set; }
        public string Status { get; set; }
        public double EncryptionTimeMs { get; set; }
        public string EncryptionStatus { get; set; }


    }
}