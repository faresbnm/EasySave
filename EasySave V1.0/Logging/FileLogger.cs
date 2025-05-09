using System;
using System.IO;
using System.Text.Json;

namespace EasySave.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _logDirectory;
        private readonly JsonSerializerOptions _jsonOptions;

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

        public void LogTransfer(string backupName, string sourcePath, string targetPath, long fileSize, double transferTimeMs)
        {
            var logEntry = new
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                BackupName = backupName,
                SourcePath = sourcePath,
                TargetPath = targetPath,
                FileSize = fileSize,
                TransferTimeMs = transferTimeMs,
                Status = transferTimeMs >= 0 ? "SUCCESS" : "FAILED"
            };

            string logFile = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");

            try
            {
                string jsonContent = File.Exists(logFile)
                    ? $"{File.ReadAllText(logFile).TrimEnd(']', '\r', '\n')},\n{JsonSerializer.Serialize(logEntry, _jsonOptions)}]"
                    : $"[{JsonSerializer.Serialize(logEntry, _jsonOptions)}]";

                File.WriteAllText(logFile, jsonContent);
            }
            catch (Exception ex)
            {
                // Fail silently or add error handling as needed
            }
        }
    }
}