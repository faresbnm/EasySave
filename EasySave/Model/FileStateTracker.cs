using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.Logging
{
    public class FileStateTracker : IStateTracker
    {
        private readonly string _stateFilePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private Dictionary<string, BackupState> _currentStates;

        public FileStateTracker()
        {
            _stateFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EasySave",
                "state.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            _currentStates = new Dictionary<string, BackupState>();
            LoadExistingStates();
        }

        private void LoadExistingStates()
        {
            try
            {
                if (File.Exists(_stateFilePath))
                {
                    string json = File.ReadAllText(_stateFilePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        _currentStates = JsonSerializer.Deserialize<Dictionary<string, BackupState>>(json, _jsonOptions)
                                        ?? new Dictionary<string, BackupState>();
                    }
                }
            }
            catch
            {
                // If we can't read the existing file, start fresh
                _currentStates = new Dictionary<string, BackupState>();
            }
        }

        public void InitializeState(string backupName)
        {
            var state = new BackupState
            {
                BackupName = backupName,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                Status = "Initializing",
                TotalFiles = 0,
                TotalSize = 0,
                FilesCopied = 0,
                SizeCopied = 0,
                RemainingFiles = 0,
                RemainingSize = 0
            };

            _currentStates[backupName] = state;
            SaveStates();
        }

        public void UpdateState(string backupName, string status, int totalFiles = 0, long totalSize = 0,
                              int filesCopied = 0, long sizeCopied = 0,
                              string currentSource = null, string currentTarget = null)
        {
            if (!_currentStates.TryGetValue(backupName, out var state))
            {
                state = new BackupState { BackupName = backupName };
            }

            state.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            state.Status = status;

            if (totalFiles > 0) state.TotalFiles = totalFiles;
            if (totalSize > 0) state.TotalSize = totalSize;
            if (filesCopied > 0) state.FilesCopied = filesCopied;
            if (sizeCopied > 0) state.SizeCopied = sizeCopied;

            state.RemainingFiles = state.TotalFiles - state.FilesCopied;
            state.RemainingSize = state.TotalSize - state.SizeCopied;

            if (currentSource != null) state.CurrentSource = currentSource;
            if (currentTarget != null) state.CurrentTarget = currentTarget;

            _currentStates[backupName] = state;
            SaveStates();
        }

        public void ClearState(string backupName)
        {
            if (_currentStates.ContainsKey(backupName))
            {
                _currentStates.Remove(backupName);
                SaveStates();
            }
        }

        private void SaveStates()
        {
            try
            {
                string json = JsonSerializer.Serialize(_currentStates, _jsonOptions);
                File.WriteAllText(_stateFilePath, json);
            }
            catch
            {
                // Fail silently or add error handling as needed
            }
        }
    }

    public class BackupState
    {
        public string BackupName { get; set; }
        public string Timestamp { get; set; }
        public string Status { get; set; } // "Initializing", "InProgress", "Completed", "Failed"
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int FilesCopied { get; set; }
        public long SizeCopied { get; set; }
        public int RemainingFiles { get; set; }
        public long RemainingSize { get; set; }
        public string CurrentSource { get; set; }
        public string CurrentTarget { get; set; }
    }
}