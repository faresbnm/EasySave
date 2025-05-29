using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Localization;
using EasySave.Logging;
using CryptoSoft;
using System.Collections.Concurrent;

namespace EasySave.Model
{
    public class BackupService
    {
        private readonly string _jsonFilePath;
        private readonly JsonSerializerOptions _jsonOptions;
        public const int MaxBackups = 5;
        private ILogger _logger;
        private readonly IStateTracker _stateTracker;
        private readonly ILocalizationService _localization;
        private PauseTokenSource _pauseTokenSource;
        private CancellationTokenSource _cancellationTokenSource;
        private object _lock = new object();
        private List<string> _encryptionExtensions = new List<string>();
        private string _encryptionKey = "123";


        public BackupService(ILocalizationService localization)
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

            _logger = new FileLogger(); // Initializing logger 
            _stateTracker = new FileStateTracker(); // Initializing state tracker
            _localization = localization; // Initializing language service

        }

        public void SetLogFormat(LogFormat format)
        {
            if (_logger != null && _logger.Format == format)
                return;

            if (format == LogFormat.Json)
            {
                _logger = new FileLogger();
            }
            else
            {
                _logger = new XmlLogger();
            }
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
                return new List<Backup>();
            }
        }

        public (bool isValid, string message) ValidateBackup(Backup newBackup)
        {
            var backups = GetAllBackups();

            // Check maximum backups limit
            //if (backups.Count >= MaxBackups)
            //{
            //   return (false, "MaximumBackupsReached");
            //}

            // Check duplicate name
            if (backups.Any(b => b.BackupName.Equals(newBackup.BackupName, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "BackupNameExists");
            }

            if (string.IsNullOrWhiteSpace(newBackup.BackupName))
            {
                return (false, "EmptyBackupName");
            }

            // Check source path
            if (!Directory.Exists(newBackup.Source))
            {
                return (false, "SourcePathNotExist");
            }

            // Check target path
            if (!Directory.Exists(newBackup.Target))
            {
                return (false, "TargetPathNotExist");
            }

            if (newBackup.Source.Equals(newBackup.Target, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "SourceAndTargetSame");
            }

            // Check backup type
            if (newBackup.Type != 1 && newBackup.Type != 2)
            {
                return (false, "InvalidBackupType");
            }

            return (true, "BackupValid");
        }

        public (bool isValid, string message) ValidateUpdatedBackup(Backup newBackup)
        {
            var backups = GetAllBackups();
            // Check source path
            if (!Directory.Exists(newBackup.Source))
            {
                return (false, "SourcePathNotExist");
            }

            if (string.IsNullOrWhiteSpace(newBackup.BackupName))
            {
                return (false, "EmptyBackupName");
            }

            if (newBackup.Source.Equals(newBackup.Target, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "SourceAndTargetSame");
            }

            // Check target path
            if (!Directory.Exists(newBackup.Target))
            {
                return (false, "TargetPathNotExist");
            }

            // Check backup type
            if (newBackup.Type != 1 && newBackup.Type != 2)
            {
                return (false, "InvalidBackupType");
            }

            return (true, "BackupValid");
        }

        public string CreateBackup(Backup backup)
        {
            try
            {
                List<Backup> backups = GetAllBackups();
                backups.Add(backup);

                string json = JsonSerializer.Serialize(backups, _jsonOptions);
                File.WriteAllText(_jsonFilePath, json);

                return "BackupCreatedSuccess";
            }
            catch (Exception ex)
            {
                return "BackupCreateFailed";
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
                    return "BackupNotFound";
                }

                backups.Remove(backupToRemove);

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(backups, options);
                File.WriteAllText(_jsonFilePath, json);

                return "BackupDeleted";
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }

        public (Backup backup, string message) GetBackupByName(string backupName)
        {
            var backups = GetAllBackups();
            var backup = backups.FirstOrDefault(b =>
                b.BackupName.Equals(backupName, StringComparison.OrdinalIgnoreCase));

            return backup == null
                ? (null, "NoBackupsAvailable")
                : (backup, "Backup found");
        }

        public string UpdateBackup(string originalName, Backup updatedBackup)
        {
            try
            {
                var backups = GetAllBackups();
                var index = backups.FindIndex(b =>
                    b.BackupName.Equals(originalName, StringComparison.OrdinalIgnoreCase));

                if (index == -1) return "BackupNotFound";

                backups[index] = updatedBackup;

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(backups, options);
                File.WriteAllText(_jsonFilePath, json);

                return "BackupUpdatedSuccess";
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }

        public string GetTimestampedFolderName(string backupName)
        {
            return $"{backupName}_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}";
        }

        public (List<string> backupNames, string error) ParseBackupSelection(string input, List<Backup> allBackups)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (null, "EmptyInput");

            if (input.Equals("all", StringComparison.OrdinalIgnoreCase))
                return (allBackups.Select(b => b.BackupName).ToList(), null);

            var results = new List<string>();
            var parts = input.Split(new[] { ',', '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return (null, "InvalidInputFormat");

            // Handle comma-separated list
            if (input.Contains(','))
            {
                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int index) && index > 0 && index <= allBackups.Count)
                        results.Add(allBackups[index - 1].BackupName);
                    else
                        return (null, "InvalidBackupNumber");
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
                return (null, "InvalidRange");
            }

            // Handle single number
            if (int.TryParse(input, out int singleIndex) && singleIndex > 0 && singleIndex <= allBackups.Count)
                return (new List<string> { allBackups[singleIndex - 1].BackupName }, null);

            return (null, "InvalidInput");
        }
        public void PauseBackup()
        {
            lock (_lock)
            {
                _pauseTokenSource?.Pause();
            }
        }

        public void ResumeBackup()
        {
            lock (_lock)
            {
                _pauseTokenSource?.Resume();
            }
        }

        public void StopBackup()
        {
            lock (_lock)
            {
                _cancellationTokenSource?.Cancel();
            }
        }
        public async Task<List<string>> ExecuteBackupsInParallel(List<string> backupNames)
        {
            // Initialiser les tokens
            lock (_lock)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _pauseTokenSource = new PauseTokenSource();
            }

            var results = new ConcurrentBag<string>();

            var tasks = backupNames.Select(async backupName =>
            {
                try
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    var (backup, message) = GetBackupByName(backupName);
                    if (backup == null)
                    {
                        results.Add(_localization.Format("BackupNotFound", backupName));
                        return;
                    }

                    _stateTracker.InitializeState(backup.BackupName);

                    if (backup.Type == 1) // Full backup
                    {
                        var fileCount = CountFiles(backup.Source);
                        _stateTracker.UpdateState(backup.BackupName, "Preparing", fileCount.Count, fileCount.TotalSize);

                        // Pause si nécessaire
                        if (_pauseTokenSource.IsPaused)
                        {
                            results.Add(_localization["BackupPaused"]);
                            await _pauseTokenSource.WaitWhilePausedAsync();
                            results.Add(_localization["BackupResumed"]);
                        }

                        await _pauseTokenSource.WaitWhilePausedAsync();

                        await CopyDirectoryAsync(backup.Source, backup.Target, backup.BackupName, true, null);
                        results.Add(_localization.Format("FullBackupSuccess", backup.BackupName));

                        _stateTracker.UpdateState(backup.BackupName, "Completed");
                    }
                    else // Differential backup
                    {
                        string originalBackupPath = backup.Type == 1 ? null : backup.Target;
                        if (originalBackupPath == null)
                        {
                            var fileCount = CountFiles(backup.Source);
                            _stateTracker.UpdateState(backup.BackupName, "Preparing", fileCount.Count, fileCount.TotalSize);

                            if (_pauseTokenSource.IsPaused)
                            {
                                results.Add(_localization["BackupPaused"]);
                                await _pauseTokenSource.WaitWhilePausedAsync();
                                results.Add(_localization["BackupResumed"]);
                            }

                            await _pauseTokenSource.WaitWhilePausedAsync();

                            await CopyDirectoryAsync(backup.Source, backup.Target, backup.BackupName, true, null);
                            results.Add(_localization.Format("DifferentialBackupSuccess", backup.BackupName));
                            _stateTracker.UpdateState(backup.BackupName, "Completed");
                        }
                        else
                        {
                            var fileCount = CountChangedFiles(backup.Source, originalBackupPath);
                            _stateTracker.UpdateState(backup.BackupName, "Preparing", fileCount.Count, fileCount.TotalSize);

                            if (_pauseTokenSource.IsPaused)
                            {
                                results.Add(_localization["BackupPaused"]);
                                await _pauseTokenSource.WaitWhilePausedAsync();
                                results.Add(_localization["BackupResumed"]);
                            }

                            await _pauseTokenSource.WaitWhilePausedAsync();

                            await CopyDirectoryAsync(backup.Source, backup.Target, backup.BackupName, false, originalBackupPath);
                            results.Add(_localization.Format("DifferentialBackupSuccess", backup.BackupName));
                            _stateTracker.UpdateState(backup.BackupName, "Completed");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    //results.Add($"Backup {backupName} cancelled.");
                }
                catch (Exception ex)
                {
                    results.Add($"Backup {backupName} failed: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks); // Attend que tous les backups se terminent

            return results.ToList();
        }

        public async Task<List<string>> ExecuteBackups(List<string> backupNames)
        {
            // Initialize control tokens
            lock (_lock)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _pauseTokenSource = new PauseTokenSource();
            }

            var results = new List<string>();

            foreach (var backupName in backupNames)
            {
                try
                {
                    // Check for cancellation before starting each backup
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    var (backup, message) = GetBackupByName(backupName);
                    if (backup == null)
                    {
                        results.Add(_localization.Format("BackupNotFound", backupName));
                        continue;
                    }

                    // Initialize state
                    _stateTracker.InitializeState(backup.BackupName);

                    if (backup.Type == 1) // Full backup
                    {
                        // Count files and total size first
                        var fileCount = CountFiles(backup.Source);
                        _stateTracker.UpdateState(backup.BackupName, "Preparing",
                            fileCount.Count, fileCount.TotalSize);

                        // Add pause check point with status
                        if (_pauseTokenSource.IsPaused)
                        {
                            results.Add(_localization["BackupPaused"]);
                            await _pauseTokenSource.WaitWhilePausedAsync();
                            results.Add(_localization["BackupResumed"]);
                        }

                        // Wait if paused before starting
                        await _pauseTokenSource.WaitWhilePausedAsync();

                        await CopyDirectoryAsync(backup.Source, backup.Target, backup.BackupName, true, null);
                        results.Add(_localization.Format("FullBackupSuccess", backup.BackupName));

                        _stateTracker.UpdateState(backup.BackupName, "Completed");
                    }
                    else // Differential backup
                    {
                        string originalBackupPath = FindOriginalFullBackup(backup.Target, backup.BackupName);
                        if (originalBackupPath == null)
                        {
                            // Count files and total size first
                            var fileCount = CountFiles(backup.Source);
                            _stateTracker.UpdateState(backup.BackupName, "Preparing",
                                fileCount.Count, fileCount.TotalSize);

                            // Add pause check point with status
                            if (_pauseTokenSource.IsPaused)
                            {
                                results.Add(_localization["BackupPaused"]);
                                await _pauseTokenSource.WaitWhilePausedAsync();
                                results.Add(_localization["BackupResumed"]);
                            }

                            // Wait if paused before starting
                            await _pauseTokenSource.WaitWhilePausedAsync();

                            await CopyDirectoryAsync(backup.Source, backup.Target, backup.BackupName, true, null);
                            results.Add(_localization.Format("DifferentialBackupSuccess", backup.BackupName));
                            _stateTracker.UpdateState(backup.BackupName, "Completed");
                        }
                        else
                        {
                            // Count changed files and total size first
                            var fileCount = CountChangedFiles(backup.Source, originalBackupPath);
                            _stateTracker.UpdateState(backup.BackupName, "Preparing",
                                fileCount.Count, fileCount.TotalSize);

                            // Add pause check point with status
                            if (_pauseTokenSource.IsPaused)
                            {
                                results.Add(_localization["BackupPaused"]);
                                await _pauseTokenSource.WaitWhilePausedAsync();
                                results.Add(_localization["BackupResumed"]);
                            }

                            // Wait if paused before starting
                            await _pauseTokenSource.WaitWhilePausedAsync();

                            await CopyDirectoryAsync(backup.Source, backup.Target, backup.BackupName, false, originalBackupPath);
                            results.Add($"Differential backup '{backup.BackupName}' executed successfully");
                            _stateTracker.UpdateState(backup.BackupName, "Completed");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    results.Add(_localization["BackupResumed"]);
                    _stateTracker.UpdateState(backupName, "Cancelled");
                }
                catch (Exception ex)
                {
                    results.Add($"Failed to execute backup '{backupName}': {ex.Message}");
                    _stateTracker.UpdateState(backupName, "Error");
                }
            }

            return results;
        }

        // Count files and total size in a directory
        private (int Count, long TotalSize) CountFiles(string directory)
        {
            int count = 0;
            long totalSize = 0;

            foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                count++;
                totalSize += new FileInfo(file).Length;
            }

            return (count, totalSize);
        }

        // Count changed files and total size in a directory compared to the last full backup
        private (int Count, long TotalSize) CountChangedFiles(string sourceDir, string lastBackupPath)
        {
            int count = 0;
            long totalSize = 0;

            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = file.Substring(sourceDir.Length + 1);
                string lastBackupFile = Path.Combine(lastBackupPath, relativePath);

                if (!File.Exists(lastBackupFile) || File.GetLastWriteTime(file) > File.GetLastWriteTime(lastBackupFile))
                {
                    count++;
                    totalSize += new FileInfo(file).Length;
                }
            }

            return (count, totalSize);
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

        public void SetEncryptionExtensions(List<string> extensions)
        {
            _encryptionExtensions = extensions ?? new List<string>();
            EncryptionService.Instance.Configure(_encryptionKey, _encryptionExtensions);
        }

        public void SetEncryptionKey(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                _encryptionKey = key;
                EncryptionService.Instance.Configure(_encryptionKey, _encryptionExtensions);
            }
        }


        private async Task CopyDirectoryAsync(string sourceDir, string targetDir, string backupName, bool isFullBackup, string lastBackupPath)
        {
            string backupTargetDir = targetDir;

            // Première passe : vérifie s’il y a des changements
            bool hasChanges = false;
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
                hasChanges = true; // Full backup always has changes
            }

            if (!hasChanges)
            {
                _stateTracker.UpdateState(backupName, "NoChanges");
                return;
            }

            Directory.CreateDirectory(backupTargetDir);
            var files = Directory.GetFiles(sourceDir);

            for (int i = 0; i < files.Length; i++)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    _stateTracker.UpdateState(backupName, "Stopping");
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                }

                if (_pauseTokenSource.IsPaused)
                {
                    _stateTracker.UpdateState(backupName, "Paused");
                    await _pauseTokenSource.WaitWhilePausedAsync();
                    _stateTracker.UpdateState(backupName, "Resuming");
                }

                string file = files[i];
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
                        _stateTracker.UpdateState(backupName, "InProgress",
                            currentSource: file, currentTarget: destFile);

                        var startTime = DateTime.Now;

                        using (var sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
                        using (var destinationStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                        {
                            await sourceStream.CopyToAsync(destinationStream);
                        }

                        // Encryption si besoin
                        double encryptionTimeMs = 0;
                        var extension = Path.GetExtension(file);
                        if (_encryptionExtensions.Contains(extension.ToLower()))
                        {
                            try
                            {
                                encryptionTimeMs = EncryptionService.Instance.EncryptFile(destFile);
                            }
                            catch
                            {
                                encryptionTimeMs = -1;
                            }
                        }

                        var endTime = DateTime.Now;
                        var fileInfo = new FileInfo(file);
                        _logger.LogTransfer(backupName, file, destFile, fileInfo.Length,
                            (endTime - startTime).TotalMilliseconds, encryptionTimeMs);

                        _stateTracker.UpdateState(backupName, "InProgress", filesCopied: i + 1, sizeCopied: fileInfo.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogTransfer(backupName, file, destFile, new FileInfo(file).Length, -1, -1);
                        _stateTracker.UpdateState(backupName, "Error", currentSource: file, currentTarget: destFile);
                    }
                }
            }
            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                await _pauseTokenSource.WaitWhilePausedAsync();

                string dirName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(backupTargetDir, dirName);
                string lastBackupSubDir = lastBackupPath != null ? Path.Combine(lastBackupPath, dirName) : null;

                await CopyDirectoryAsync(subDir, destSubDir, backupName, isFullBackup, lastBackupSubDir);
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

            if (!hasChanges)
            {
                _stateTracker.UpdateState(backupName, "NoChanges");
                return; // No changes, skip backup
            }



            // Second pass: actually copy files
            Directory.CreateDirectory(backupTargetDir);
            var files = Directory.GetFiles(sourceDir);
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
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
                        // Update state before copying
                        _stateTracker.UpdateState(backupName, "InProgress",
                            currentSource: file, currentTarget: destFile);

                        var startTime = DateTime.Now;
                        File.Copy(file, destFile, true);

                        // Encrypt if needed
                        double encryptionTimeMs = 0;
                        var extension = Path.GetExtension(file);
                        if (_encryptionExtensions.Contains(extension.ToLower()))
                        {
                            try
                            {
                                var fileManager = new FileManager(destFile, _encryptionKey);
                                encryptionTimeMs = fileManager.TransformFile();
                            }
                            catch
                            {
                                encryptionTimeMs = -1; // Indicates encryption failure
                            }
                        }
                        var endTime = DateTime.Now;

                        // Log the file transfer
                        var fileInfo = new FileInfo(file);
                        _logger.LogTransfer(
                            backupName,
                            file,
                            destFile,
                            fileInfo.Length,
                            (endTime - startTime).TotalMilliseconds,
                            encryptionTimeMs
                        );

                        // Update state after successful copy
                        _stateTracker.UpdateState(backupName, "InProgress",
                            filesCopied: i + 1, sizeCopied: fileInfo.Length);
                    }
                    catch (Exception ex)
                    {
                        // Log failed transfer with negative transfer time
                        _logger.LogTransfer(
                            backupName,
                            file,
                            destFile,
                            new FileInfo(file).Length,
                            -1, // Negative value indicates failure
                            -1 // Negative value indicates failure
                        );

                        _stateTracker.UpdateState(backupName, "Error",
                            currentSource: file, currentTarget: destFile);
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