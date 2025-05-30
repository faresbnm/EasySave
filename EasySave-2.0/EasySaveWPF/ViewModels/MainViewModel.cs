using EasySave.Model;
using EasySave.Localization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using System.Windows.Forms;
using System.Linq;
using EasySave.Logging;
using EasySaveWPF.Settings;
using System.Diagnostics;
using System.Timers;

namespace EasySaveWPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly BackupService _backupService;
        private readonly ILocalizationService _localization;
        private ObservableCollection<Backup> _backups;
        private bool _isAddBackupDialogOpen;
        private bool _isUpdateBackupDialogOpen;
        private bool _isBackupRunning;
        private bool _isBackupPaused;
        private CancellationTokenSource _cancellationTokenSource;
        private PauseTokenSource _pauseTokenSource;
        private bool _canPauseBackup;
        private bool _canResumeBackup;
        private bool _canStopBackup;
        private bool _wasBackupStopped;
        private readonly SettingsService _settingsService = new();
        private UserSettings _currentSettings;
        private System.Timers.Timer _monitoringTimer;
        private bool _softwareWasRunning;
        private readonly object _monitoringLock = new object();
        private bool _isPausedBySoftware;
        private ObservableCollection<string> _priorityExtensions { get; set; } = new ObservableCollection<string> { ".txt" };


        public bool IsSettingsDialogOpen { get; set; }
        public int SelectedLogFormat { get; set; } = 0; // 0 = JSON, 1 = XML
        public string BusinessSoftwareName { get; set; } = "Calculator";
        public ObservableCollection<string> EncryptionExtensions { get; set; } = new ObservableCollection<string> { ".txt", ".pdf", ".docx" };
        public string NewExtension { get; set; }

        public string UpdateBackupButtonText => _localization["UpdateBackupButtonText"];
        public string DeleteBackupButtonText => _localization["DeleteBackupButtonText"];
        public string ExecuteBackupButtonText => _localization["ExecuteBackupButtonText"];
        public string SettingsButtonText => _localization["Settings"];
        public string AddExtensionButtonText => _localization["AddExtensionToEncrypt"];
        public string RemoveExtensionButtonText => _localization["RemoveExtensionToEncrypt"];
        public string ExtensionsToEncryptText => _localization["ExtensionsToEncrypt"];
        public string EncryptionKeyText => _localization["EncryptionKey"];

        private string _encryptionKey = "123"; // Default key
        public string NewPriorityExtension { get; set; }
        public string PriorityExtensionsText => _localization["PriorityExtensions"];
        public string AddPriorityExtensionButtonText => _localization["AddPriorityExtension"];
        public string RemovePriorityExtensionButtonText => _localization["RemovePriorityExtension"];


        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel(ILocalizationService localizationService, BackupService backupService)
        {
            _localization = localizationService;
            _backupService = backupService;

            // Initialize settings service and load settings
            _settingsService = new SettingsService();
            _currentSettings = _settingsService.Load();

            // Initialize the monitoring timer (check every 5 seconds)
            _monitoringTimer = new System.Timers.Timer(2000);
            _monitoringTimer.Elapsed += MonitorBusinessSoftware;
            _monitoringTimer.AutoReset = true;
            _monitoringTimer.Start();

            // Apply loaded settings to bound properties
            SelectedLogFormat = _currentSettings.SelectedLogFormat;
            BusinessSoftwareName = _currentSettings.BusinessSoftwareName;
            EncryptionExtensions = new ObservableCollection<string>(_currentSettings.EncryptionExtensions ?? new());
            EncryptionKey = _currentSettings.EncryptionKey;
            PriorityExtensions = new ObservableCollection<string>(_currentSettings.PriorityExtensions ?? new());

            // Language 
            if (!string.IsNullOrEmpty(_currentSettings.Language))
                ChangeLanguage(_currentSettings.Language);

            ChangeLanguageCommand = new RelayCommand<string>(ChangeLanguage);
            LoadBackupsCommand = new RelayCommand(LoadBackups);
            AddBackupCommand = new RelayCommand(AddBackup);
            OpenAddBackupDialogCommand = new RelayCommand(OpenAddBackupDialog);
            CloseAddBackupDialogCommand = new RelayCommand(CloseAddBackupDialog);
            BrowseSourceCommand = new RelayCommand(BrowseSource);
            BrowseTargetCommand = new RelayCommand(BrowseTarget);
            OpenUpdateBackupDialogCommand = new RelayCommand(OpenUpdateBackupDialog);
            CloseUpdateBackupDialogCommand = new RelayCommand(CloseUpdateBackupDialog);
            UpdateBackupCommand = new RelayCommand(UpdateBackup);
            DeleteBackupCommand = new RelayCommand(DeleteBackup);
            ExecuteBackupCommand = new RelayCommand(ExecuteBackup);
            OpenSettingsDialogCommand = new RelayCommand(OpenSettingsDialog);
            CloseSettingsDialogCommand = new RelayCommand(CloseSettingsDialog);
            ApplySettingsCommand = new RelayCommand(ApplySettings);
            AddExtensionCommand = new RelayCommand(AddExtension);
            RemoveExtensionCommand = new RelayCommand<string>(RemoveExtension);
            PauseBackupCommand = new RelayCommand(PauseBackup, () => CanPauseBackup);
            ResumeBackupCommand = new RelayCommand(ResumeBackup, () => CanResumeBackup);
            StopBackupCommand = new RelayCommand(StopBackup, () => CanStopBackup);
            AddPriorityExtensionCommand = new RelayCommand(AddPriorityExtension);
            RemovePriorityExtensionCommand = new RelayCommand<string>(RemovePriorityExtension);
            CanPauseBackup = false;
            CanResumeBackup = false;
            CanStopBackup = false;


            LoadBackups();
        }

        public ObservableCollection<Backup> Backups
        {
            get => _backups;
            set
            {
                _backups = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NoBackupsText));
                OnPropertyChanged(nameof(NoBackupsVisibility));
            }
        }

        public ObservableCollection<string> PriorityExtensions
        {
            get => _priorityExtensions;
            set
            {
                _priorityExtensions = value;
                OnPropertyChanged();
            }
        }

        public Backup SelectedBackup { get; set; }
        public ObservableCollection<Backup> SelectedBackups { get; set; } = new ObservableCollection<Backup>();

        public string EncryptionKey
        {
            get => _encryptionKey;
            set
            {
                _encryptionKey = value;
                OnPropertyChanged();
            }
        }
        public bool IsAddBackupDialogOpen
        {
            get => _isAddBackupDialogOpen;
            set { _isAddBackupDialogOpen = value; OnPropertyChanged(); }
        }

        public bool IsUpdateBackupDialogOpen
        {
            get => _isUpdateBackupDialogOpen;
            set { _isUpdateBackupDialogOpen = value; OnPropertyChanged(); }
        }

        public bool CanPauseBackup
        {
            get => _canPauseBackup;
            set { _canPauseBackup = value; OnPropertyChanged(); }
        }

        public bool CanResumeBackup
        {
            get => _canResumeBackup;
            set { _canResumeBackup = value; OnPropertyChanged(); }
        }

        public bool CanStopBackup
        {
            get => _canStopBackup;
            set { _canStopBackup = value; OnPropertyChanged(); }
        }



        public string NewBackupName { get; set; }
        public string NewBackupSource { get; set; }
        public string NewBackupTarget { get; set; }
        public int NewBackupType { get; set; } = 1;

        public string UpdatedBackupName { get; set; }
        public string UpdatedBackupSource { get; set; }
        public string UpdatedBackupTarget { get; set; }
        public int UpdatedBackupType { get; set; }

        public string NoBackupsText => _localization["NoBackupJobs"];
        public Visibility NoBackupsVisibility => Backups?.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        public string WindowTitle => _localization["WelcomeTitleWPF"];
        public string BackupListHeader => _localization["BackupJobsList"];
        public string NameHeader => _localization["BackupDetailName"];
        public string SourceHeader => _localization["BackupDetailSource"];
        public string TargetHeader => _localization["BackupDetailTarget"];
        public string TypeHeader => _localization["BackupDetailType"];
        public string RefreshButtonText => _localization["MenuOption1"];
        public string AddBackupButtonText => _localization["MenuOption2"];
        public string CancelButtonText => _localization["cancel"];
        public string SaveButtonText => _localization["ok"];
        public string SetLanguage => _localization["SetLanguage"];
        public string FullBackupText => _localization["BackupTypeFull"];
        public string DifferentialBackupText => _localization["BackupTypeDifferential"];
        public string ExtensionText => _localization["ExtensionText"];
        public string LogFormatText => _localization["LogFormatText"];
        public string SoftwareText => _localization["SoftwareText"];
        public string SelectBackupToUpdate => _localization["SelectBackupToUpdate"];

        public ICommand ChangeLanguageCommand { get; }
        public ICommand LoadBackupsCommand { get; }
        public ICommand AddBackupCommand { get; }
        public ICommand OpenAddBackupDialogCommand { get; }
        public ICommand CloseAddBackupDialogCommand { get; }
        public ICommand BrowseSourceCommand { get; }
        public ICommand BrowseTargetCommand { get; }
        public ICommand OpenUpdateBackupDialogCommand { get; }
        public ICommand CloseUpdateBackupDialogCommand { get; }
        public ICommand UpdateBackupCommand { get; }
        public ICommand DeleteBackupCommand { get; }
        public ICommand ExecuteBackupCommand { get; }
        public ICommand OpenSettingsDialogCommand { get; }
        public ICommand CloseSettingsDialogCommand { get; }
        public ICommand ApplySettingsCommand { get; }
        public ICommand BrowseUpdatedSourceCommand => new RelayCommand(BrowseUpdatedSource);
        public ICommand BrowseUpdatedTargetCommand => new RelayCommand(BrowseUpdatedTarget);
        public ICommand AddExtensionCommand { get; }
        public ICommand RemoveExtensionCommand { get; }
        public ICommand PauseBackupCommand { get; }
        public ICommand ResumeBackupCommand { get; }
        public ICommand StopBackupCommand { get; }
        public ICommand AddPriorityExtensionCommand { get; }
        public ICommand RemovePriorityExtensionCommand { get; }

        private void ChangeLanguage(string languageCode)
        {
            _localization.SetLanguage(languageCode);
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(BackupListHeader));
            OnPropertyChanged(nameof(NameHeader));
            OnPropertyChanged(nameof(SourceHeader));
            OnPropertyChanged(nameof(TargetHeader));
            OnPropertyChanged(nameof(TypeHeader));
            OnPropertyChanged(nameof(RefreshButtonText));
            OnPropertyChanged(nameof(NoBackupsText));
            OnPropertyChanged(nameof(AddBackupButtonText));
            OnPropertyChanged(nameof(CancelButtonText));
            OnPropertyChanged(nameof(SaveButtonText));
            OnPropertyChanged(nameof(UpdateBackupButtonText));
            OnPropertyChanged(nameof(DeleteBackupButtonText));
            OnPropertyChanged(nameof(ExecuteBackupButtonText));
            OnPropertyChanged(nameof(SettingsButtonText));
            OnPropertyChanged(nameof(FullBackupText));
            OnPropertyChanged(nameof(DifferentialBackupText));
            OnPropertyChanged(nameof(ExtensionText));
            OnPropertyChanged(nameof(SetLanguage));
            OnPropertyChanged(nameof(LogFormatText));
            OnPropertyChanged(nameof(SoftwareText));
            OnPropertyChanged(nameof(SelectBackupToUpdate));
            OnPropertyChanged(nameof(AddExtensionButtonText));
            OnPropertyChanged(nameof(RemoveExtensionButtonText));
            OnPropertyChanged(nameof(ExtensionsToEncryptText));
            OnPropertyChanged(nameof(EncryptionKeyText));

        }

        private void AddExtension()
        {
            if (!string.IsNullOrWhiteSpace(NewExtension))
            {
                if (!NewExtension.StartsWith("."))
                {
                    NewExtension = "." + NewExtension;
                }

                if (!EncryptionExtensions.Contains(NewExtension))
                {
                    EncryptionExtensions.Add(NewExtension);
                    NewExtension = string.Empty;
                    OnPropertyChanged(nameof(NewExtension));
                }
                else
                {
                    System.Windows.MessageBox.Show(_localization["ExtensionAlreadyExists"], _localization["Error"], MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        private void AddPriorityExtension()
        {
            if (!string.IsNullOrWhiteSpace(NewPriorityExtension))
            {
                if (!NewPriorityExtension.StartsWith("."))
                {
                    NewPriorityExtension = "." + NewPriorityExtension;
                }

                if (!PriorityExtensions.Contains(NewPriorityExtension))
                {
                    PriorityExtensions.Add(NewPriorityExtension);
                    NewPriorityExtension = string.Empty;
                    OnPropertyChanged(nameof(NewPriorityExtension));
                }
                else
                {
                    System.Windows.MessageBox.Show(_localization["ExtensionAlreadyExists"], _localization["Error"], MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void RemovePriorityExtension(string extension)
        {
            if (PriorityExtensions.Contains(extension))
            {
                PriorityExtensions.Remove(extension);
            }
        }

        private void MonitorBusinessSoftware(object sender, ElapsedEventArgs e)
        {
            lock (_monitoringLock)
            {
                if (string.IsNullOrWhiteSpace(BusinessSoftwareName))
                    return;

                bool isRunning = IsProcessRunning(BusinessSoftwareName);

                // If state changed
                if (isRunning != _softwareWasRunning)
                {
                    _softwareWasRunning = isRunning;

                    // Run on UI thread
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (isRunning)
                        {
                            if (_isBackupRunning && !_isBackupPaused)
                            {
                                _isPausedBySoftware = true; // Mark as software-paused
                                PauseBackup();
                            }
                        }
                        else
                        {
                            if (_isBackupRunning && _isBackupPaused && _isPausedBySoftware)
                            {
                                _isPausedBySoftware = false; // Clear software-paused flag
                                ResumeBackup();
                            }
                        }
                    });
                }
            }
        }

        private bool IsProcessRunning(string processName)
        {
            // Remove .exe if present for more flexible matching
            processName = processName.Replace(".exe", "").Replace(".EXE", "");

            if (string.IsNullOrWhiteSpace(processName))
                return false;

            return Process.GetProcesses()
                .Any(p => p.ProcessName.Contains(processName, StringComparison.OrdinalIgnoreCase));
        }

        private void PauseBackup()
        {
            try
            {
                _backupService.PauseBackup();
                CanPauseBackup = false;
                CanResumeBackup = true;
                CanStopBackup = true;
                _isBackupPaused = true;

                System.Windows.MessageBox.Show(_localization["BackupPaused"], _localization["Status"]);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(_localization["PauseFailed"], _localization["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResumeBackup()
        {
            try
            {
                // Prevent manual resume if paused by software
                if (_isPausedBySoftware)
                {
                    System.Windows.MessageBox.Show(
                       string.Format(_localization["CannotResumeSoftwarePause"], BusinessSoftwareName),
                       _localization["Warning"],
                       MessageBoxButton.OK,
                       MessageBoxImage.Warning);
                    return;
                }

                _backupService.ResumeBackup();
                CanPauseBackup = true;
                CanResumeBackup = false;
                CanStopBackup = true;
                _isBackupPaused = false;
                System.Windows.MessageBox.Show(_localization["BackupResumed"], _localization["Status"]);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(_localization["ResumeFailed"], _localization["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopBackup()
        {
            try
            {
                _backupService.StopBackup();
                CanPauseBackup = false;
                CanResumeBackup = false;
                CanStopBackup = false;
                _isBackupRunning = false;
                _isBackupPaused = false;
                _wasBackupStopped = true;
                System.Windows.MessageBox.Show(_localization["BackupStopped"], _localization["Status"]);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(_localization["StopFailed"], _localization["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveExtension(string extension)
        {
            if (EncryptionExtensions.Contains(extension))
            {
                EncryptionExtensions.Remove(extension);
            }
        }

        private void LoadBackups()
        {
            var backups = _backupService.GetAllBackups();
            Backups = new ObservableCollection<Backup>(backups);

            foreach (var backup in Backups)
            {
                backup.TypeDisplay = backup.Type == 1
                    ? _localization["BackupTypeFull"]
                    : _localization["BackupTypeDifferential"];
            }
        }

        private void OpenAddBackupDialog()
        {
            NewBackupName = string.Empty;
            NewBackupSource = string.Empty;
            NewBackupTarget = string.Empty;
            NewBackupType = 1;
            OnPropertyChanged(nameof(NewBackupName));
            OnPropertyChanged(nameof(NewBackupSource));
            OnPropertyChanged(nameof(NewBackupTarget));
            OnPropertyChanged(nameof(NewBackupType));
            IsAddBackupDialogOpen = true;
        }

        private void CloseAddBackupDialog()
        {
            IsAddBackupDialogOpen = false;
        }

        private void BrowseSource()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    NewBackupSource = dialog.SelectedPath;
                    OnPropertyChanged(nameof(NewBackupSource));
                }
            }
        }

        private void BrowseTarget()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    NewBackupTarget = dialog.SelectedPath;
                    OnPropertyChanged(nameof(NewBackupTarget));
                }
            }
        }

        private void AddBackup()
        {
            var newBackup = new Backup
            {
                BackupName = NewBackupName,
                Source = NewBackupSource,
                Target = NewBackupTarget,
                Type = NewBackupType + 1
            };

            var (isValid, message) = _backupService.ValidateBackup(newBackup);
            if (!isValid)
            {
                System.Windows.MessageBox.Show(_localization[message], _localization["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = _backupService.CreateBackup(newBackup);
            if (result == "BackupCreatedSuccess")
            {
                LoadBackups();
                IsAddBackupDialogOpen = false;
                System.Windows.MessageBox.Show(_localization[result], _localization["Success"], MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show(_localization[result], _localization["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenUpdateBackupDialog()
        {
            if (SelectedBackup == null)
            {
                System.Windows.MessageBox.Show(SelectBackupToUpdate);
                return;
            }

            UpdatedBackupName = SelectedBackup.BackupName;
            UpdatedBackupSource = SelectedBackup.Source;
            UpdatedBackupTarget = SelectedBackup.Target;
            UpdatedBackupType = SelectedBackup.Type;

            OnPropertyChanged(nameof(UpdatedBackupName));
            OnPropertyChanged(nameof(UpdatedBackupSource));
            OnPropertyChanged(nameof(UpdatedBackupTarget));
            OnPropertyChanged(nameof(UpdatedBackupType));

            IsUpdateBackupDialogOpen = true;
        }

        private void CloseUpdateBackupDialog()
        {
            IsUpdateBackupDialogOpen = false;
        }

        private void UpdateBackup()
        {
            var updated = new Backup
            {
                BackupName = UpdatedBackupName,
                Source = UpdatedBackupSource,
                Target = UpdatedBackupTarget,
                Type = UpdatedBackupType
            };

            var (isValid, message) = _backupService.ValidateUpdatedBackup(updated);

            if (!isValid)
            {
                System.Windows.MessageBox.Show(_localization[message], _localization["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = _backupService.UpdateBackup(SelectedBackup.BackupName, updated);

            if (result == "BackupUpdatedSuccess")
            {
                LoadBackups();
                IsUpdateBackupDialogOpen = false;
                System.Windows.MessageBox.Show(_localization[result], _localization["Success"], MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show(_localization[result], _localization["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteBackup()
        {
            if (SelectedBackup == null)
            {
                System.Windows.MessageBox.Show(_localization["SelectBackupToDelete"], _localization["Error"], MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = System.Windows.MessageBox.Show(
                string.Format(_localization["ConfirmDelete"], SelectedBackup.BackupName),
                _localization["Confirm"],
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            var result = _backupService.DeleteBackup(SelectedBackup.BackupName);

            if (result == "BackupDeleted")
            {
                LoadBackups();
                System.Windows.MessageBox.Show(_localization["BackupDeletedSuccess"], _localization["Success"], MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show(_localization["Error"] + " : " + result);
            }
        }

        private async void ExecuteBackup()
        {
            var selected = SelectedBackups?.ToList() ?? new List<Backup>();
            if (selected.Count == 0)
            {
                System.Windows.MessageBox.Show(_localization["SelectBackupToExecute"], _localization["Error"], MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isSoftwareRunning = IsProcessRunning(BusinessSoftwareName);
            if (isSoftwareRunning)
            {
                System.Windows.MessageBox.Show(
                    string.Format(_localization["CannotResumeSoftwarePause"], BusinessSoftwareName),
                    _localization["Warning"],
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            CanPauseBackup = true;
            CanStopBackup = true;
            CanResumeBackup = false;
            _isBackupRunning = true;
            _wasBackupStopped = false;

            try
            {
                var names = selected.Select(b => b.BackupName).ToList();

                // ⚠️ Lancer dans un thread de fond pour ne pas bloquer l’UI
                var results = await Task.Run(() =>
                {
                    return _backupService.ExecuteBackupsInParallel(names);
                });

                if (results != null && !_wasBackupStopped)
                {
                    System.Windows.MessageBox.Show(_localization["ExecutionResult"], _localization["Success"], MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (OperationCanceledException)
            {
                // StopBackup gère déjà le message
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(_localization["ExecutionFailed"] + ": " + ex.Message, _localization["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CanPauseBackup = false;
                CanStopBackup = false;
                CanResumeBackup = false;
                _isBackupRunning = false;
            }
        }


        private void BrowseUpdatedSource()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    UpdatedBackupSource = dialog.SelectedPath;
                    OnPropertyChanged(nameof(UpdatedBackupSource));
                }
            }
        }

        private void BrowseUpdatedTarget()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    UpdatedBackupTarget = dialog.SelectedPath;
                    OnPropertyChanged(nameof(UpdatedBackupTarget));
                }
            }
        }

        private void OpenSettingsDialog()
        {
            IsSettingsDialogOpen = true;
            OnPropertyChanged(nameof(IsSettingsDialogOpen));
        }

        private void CloseSettingsDialog()
        {
            IsSettingsDialogOpen = false;
            OnPropertyChanged(nameof(IsSettingsDialogOpen));
        }

        private void ApplySettings()
        {
            var format = SelectedLogFormat == 0 ? LogFormat.Json : LogFormat.Xml;

            _backupService.SetLogFormat(format);
            _backupService.SetEncryptionExtensions(EncryptionExtensions.ToList());
            _backupService.SetEncryptionKey(EncryptionKey);
            _backupService.SetPriorityExtensions(PriorityExtensions.ToList());

            // Update and save settings
            _currentSettings = new UserSettings
            {
                SelectedLogFormat = SelectedLogFormat,
                BusinessSoftwareName = BusinessSoftwareName,
                EncryptionExtensions = EncryptionExtensions.ToList(),
                EncryptionKey = EncryptionKey,
                PriorityExtensions = PriorityExtensions.ToList(),
                Language = _localization.CurrentLanguage // Assuming you store this
            };

            _settingsService.Save(_currentSettings);

            // Reset monitoring state when settings change
            lock (_monitoringLock)
            {
                _softwareWasRunning = IsProcessRunning(BusinessSoftwareName);
            }


            // Close settings dialog
            IsSettingsDialogOpen = false;
            OnPropertyChanged(nameof(IsSettingsDialogOpen));

            // Notify user
            System.Windows.MessageBox.Show(_localization["SettingsSavedSuccess"], _localization["Success"], MessageBoxButton.OK, MessageBoxImage.Information);
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}