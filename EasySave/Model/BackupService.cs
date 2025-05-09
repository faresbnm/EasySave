using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Logging;

namespace EasySave.Model
{
    internal class BackupService
    {
        private readonly string _jsonFilePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private const int MaxBackups = 5;
        private readonly ILogger _logger;


        public BackupService()
        {
            string directoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "9raya",
                "Livrable",
                "Backups");

            Directory.CreateDirectory(directoryPath);
            _jsonFilePath = Path.Combine(directoryPath, "backups.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            };

            _logger = new FileLogger(); 
        }

        public List<Backup> GetAllBackups()
        {
            try
            {
                if (File.Exists(_jsonFilePath))
                {
                    string json = File.ReadAllText(_jsonFilePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        return JsonSerializer.Deserialize<List<Backup>>(json, _jsonOptions) ?? new List<Backup>();
                    }
                }
                return new List<Backup>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading backups: {ex.Message}");
                return new List<Backup>();
            }
        }

        public (bool isValid, string message) ValidateBackup(Backup newBackup)
        {
            var backups = GetAllBackups();

            // Check maximum backups limit
            if (backups.Count >= MaxBackups)
            {
                return (false, $"Maximum of {MaxBackups} backups reached");
            }

            // Check duplicate name
            if (backups.Any(b => b.BackupName.Equals(newBackup.BackupName, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Backup name already exists");
            }

            // Check source path
            if (!Directory.Exists(newBackup.Source))
            {
                return (false, "Source path does not exist");
            }

            // Check target path
            if (!Directory.Exists(newBackup.Target))
            {
                return (false, "Target path does not exist");
            }

            // Check backup type
            if (newBackup.Type != 1 && newBackup.Type != 2)
            {
                return (false, "Invalid backup type (must be 1 or 2)");
            }

            return (true, "Backup is valid");
        }

        public (bool isValid, string message) ValidateUpdatedBackup(Backup newBackup)
        {
            var backups = GetAllBackups();
            // Check source path
            if (!Directory.Exists(newBackup.Source))
            {
                return (false, "Source path does not exist");
            }

            // Check target path
            if (!Directory.Exists(newBackup.Target))
            {
                return (false, "Target path does not exist");
            }

            // Check backup type
            if (newBackup.Type != 1 && newBackup.Type != 2)
            {
                return (false, "Invalid backup type (must be 1 or 2)");
            }

            return (true, "Backup is valid");
        }

        public string CreateBackup(Backup backup)
        {
            try
            {
                List<Backup> backups = GetAllBackups();
                backups.Add(backup);

                string json = JsonSerializer.Serialize(backups, _jsonOptions);
                File.WriteAllText(_jsonFilePath, json);

                return "Backup created successfully!";
            }
            catch (Exception ex)
            {
                return $"Failed to create backup: {ex.Message}";
            }
        }

        public string DeleteBackup(string backupName)
        {
            try
            {
                var backups = GetAllBackups();
                var backupToRemove = backups.FirstOrDefault(b =>
                    b.BackupName.Equals(backupName, StringComparison.OrdinalIgnoreCase));

                if (backupToRemove == null)
                {
                    return $"Backup '{backupName}' not found";
                }

                backups.Remove(backupToRemove);

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(backups, options);
                File.WriteAllText(_jsonFilePath, json);

                return $"Backup '{backupName}' deleted successfully";
            }
            catch (Exception ex)
            {
                return $"Failed to delete backup: {ex.Message}";
            }
        }

        public (Backup backup, string message) GetBackupByName(string backupName)
        {
            var backups = GetAllBackups();
            var backup = backups.FirstOrDefault(b =>
                b.BackupName.Equals(backupName, StringComparison.OrdinalIgnoreCase));

            return backup == null
                ? (null, $"Backup '{backupName}' not found")
                : (backup, "Backup found");
        }

        public string UpdateBackup(string originalName, Backup updatedBackup)
        {
            try
            {
                var backups = GetAllBackups();
                var index = backups.FindIndex(b =>
                    b.BackupName.Equals(originalName, StringComparison.OrdinalIgnoreCase));

                if (index == -1) return $"Backup '{originalName}' not found";

                backups[index] = updatedBackup;

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(backups, options);
                File.WriteAllText(_jsonFilePath, json);

                return $"Backup '{updatedBackup.BackupName}' updated successfully";
            }
            catch (Exception ex)
            {
                return $"Failed to update backup: {ex.Message}";
            }
        }

        public string GetTimestampedFolderName(string backupName)
        {
            return $"{backupName}_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}";
        }

        public (List<string> backupNames, string error) ParseBackupSelection(string input, List<Backup> allBackups)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (null, "Empty input");

            if (input.Equals("all", StringComparison.OrdinalIgnoreCase))
                return (allBackups.Select(b => b.BackupName).ToList(), null);

            var results = new List<string>();
            var parts = input.Split(new[] { ',', '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return (null, "Invalid input format");

            // Handle comma-separated list
            if (input.Contains(','))
            {
                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int index) && index > 0 && index <= allBackups.Count)
                        results.Add(allBackups[index - 1].BackupName);
                    else
                        return (null, $"Invalid backup number: {part}");
                }
                return (results, null);
            }

            // Handle range (e.g., 2-4)
            if (input.Contains('-') && parts.Length == 2)
            {
                if (int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end) &&
                    start > 0 && end <= allBackups.Count && start <= end)
                {
                    for (int i = start; i <= end; i++)
                        results.Add(allBackups[i - 1].BackupName);
                    return (results, null);
                }
                return (null, $"Invalid range: {input}");
            }

            // Handle single number
            if (int.TryParse(input, out int singleIndex) && singleIndex > 0 && singleIndex <= allBackups.Count)
                return (new List<string> { allBackups[singleIndex - 1].BackupName }, null);

            return (null, $"Invalid input: {input}");
        }

        public List<string> ExecuteBackups(List<string> backupNames)
        {
            var results = new List<string>();

            foreach (var backupName in backupNames)
            {
                try
                {
                    var (backup, message) = GetBackupByName(backupName);
                    if (backup == null)
                    {
                        results.Add($"Backup '{backupName}' not found");
                        continue;
                    }

                    if (backup.Type == 1) // Full backup
                    {
                        CopyDirectory(backup.Source, backup.Target, backup.BackupName, true, null);
                        results.Add($"Full backup '{backup.BackupName}' executed successfully");
                    }
                    else // Differential backup
                    {
                        string originalBackupPath = FindOriginalFullBackup(backup.Target, backup.BackupName);
                        if (originalBackupPath == null)
                        {
                            CopyDirectory(backup.Source, backup.Target, backup.BackupName, true, null);
                            results.Add($"Initial full backup '{backup.BackupName}' executed successfully (no base backup found)");
                        }
                        else
                        {
                            CopyDirectory(backup.Source, backup.Target, backup.BackupName, false, originalBackupPath);
                            results.Add($"Differential backup '{backup.BackupName}' executed successfully");
                        }
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"Failed to execute backup '{backupName}': {ex.Message}");
                }
            }

            return results;
        }

        private string FindOriginalFullBackup(string targetDir, string backupName)
        {
            try
            {
                return Directory.GetDirectories(targetDir)
                    .Where(d => Path.GetFileName(d).StartsWith(backupName + "_"))
                    .OrderBy(d => Directory.GetCreationTime(d)) // Get the oldest backup
                    .FirstOrDefault(); // This will be the original full backup
            }
            catch
            {
                return null;
            }
        }

        private void CopyDirectory(string sourceDir, string targetDir, string backupName, bool isFullBackup, string lastBackupPath)
        {
            string timestampedFolder = GetTimestampedFolderName(backupName);
            string backupTargetDir = Path.Combine(targetDir, timestampedFolder);

            bool hasChanges = false;

            // First pass: check if there are any changes
            if (!isFullBackup && lastBackupPath != null)
            {
                hasChanges = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories)
                    .Any(file => {
                        string relativePath = file.Substring(sourceDir.Length + 1);
                        string lastBackupFile = Path.Combine(lastBackupPath, relativePath);
                        return !File.Exists(lastBackupFile) ||
                               File.GetLastWriteTime(file) > File.GetLastWriteTime(lastBackupFile);
                    });
            }
            else
            {
                hasChanges = true; // Full backup always has "changes"
            }

            if (!hasChanges) return; // No changes, skip backup

            // Second pass: actually copy files
            Directory.CreateDirectory(backupTargetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(backupTargetDir, fileName);

                bool shouldCopy = isFullBackup;

                if (!isFullBackup)
                {
                    string lastBackupFile = Path.Combine(lastBackupPath, fileName);
                    shouldCopy = !File.Exists(lastBackupFile) ||
                                File.GetLastWriteTime(file) > File.GetLastWriteTime(lastBackupFile);
                }

                if (shouldCopy)
                {
                    try
                    {
                        var startTime = DateTime.Now;
                        File.Copy(file, destFile, true);
                        var endTime = DateTime.Now;

                        // Log the file transfer
                        var fileInfo = new FileInfo(file);
                        _logger.LogTransfer(
                            backupName,
                            file,
                            destFile,
                            fileInfo.Length,
                            (endTime - startTime).TotalMilliseconds
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log failed transfer with negative transfer time
                        _logger.LogTransfer(
                            backupName,
                            file,
                            destFile,
                            new FileInfo(file).Length,
                            -1 // Negative value indicates failure
                        );
                    }
                }
            }

            // Handle subdirectories
            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(backupTargetDir, dirName);
                string lastBackupSubDir = lastBackupPath != null ? Path.Combine(lastBackupPath, dirName) : null;

                CopyDirectory(subDir, destSubDir, backupName, isFullBackup, lastBackupSubDir);
            }
        }
    }
}