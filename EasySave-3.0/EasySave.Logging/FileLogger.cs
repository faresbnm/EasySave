﻿using System;
using System.IO;
using System.Text.Json;

namespace EasySave.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _logDirectory;
        private readonly JsonSerializerOptions _jsonOptions;
        public LogFormat Format { get; set; } = LogFormat.Json;

        public FileLogger()
        {
            _logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EasySave",
                "Logs");

            Directory.CreateDirectory(_logDirectory);

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public void LogTransfer(string backupName, string sourcePath, string targetPath,
                              long fileSize, double transferTimeMs, double encryptionTimeMs)
        {
            var logEntry = new
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

            string logFile = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");

            try
            {
                // Use file locking to prevent concurrent access
                lock (typeof(FileLogger)) // Global lock for all FileLogger instances
                {
                    string jsonContent;
                    if (File.Exists(logFile))
                    {
                        var existingContent = File.ReadAllText(logFile).TrimEnd(']', '\r', '\n');
                        jsonContent = $"{existingContent},\n{JsonSerializer.Serialize(logEntry, _jsonOptions)}]";
                    }
                    else
                    {
                        jsonContent = $"[{JsonSerializer.Serialize(logEntry, _jsonOptions)}]";
                    }

                    File.WriteAllText(logFile, jsonContent);
                }
            }
            catch (Exception ex)
            {
                // Consider logging this error somewhere
            }
        }
    }
}