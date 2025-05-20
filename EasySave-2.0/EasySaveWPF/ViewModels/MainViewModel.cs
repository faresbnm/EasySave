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

        public bool IsSettingsDialogOpen { get; set; }
        public int SelectedLogFormat { get; set; } = 0; // 0 = JSON, 1 = XML
        public string BusinessSoftwareName { get; set; } = "Calculator";

        public string UpdateBackupButtonText => _localization["UpdateBackupButtonText"];
        public string DeleteBackupButtonText => _localization["DeleteBackupButtonText"];
        public string ExecuteBackupButtonText => _localization["ExecuteBackupButtonText"];
        public string SettingsButtonText => _localization["Settings"];

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel(ILocalizationService localizationService, BackupService backupService)
        {
            _localization = localizationService;
            _backupService = backupService;

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

        public Backup SelectedBackup { get; set; }
        public ObservableCollection<Backup> SelectedBackups { get; set; } = new ObservableCollection<Backup>();

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

            var (isValid, message) = _backupService.ValidateBackup(updated);

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

        private void ExecuteBackup()
        {
            var selected = SelectedBackups?.ToList() ?? new List<Backup>();

            if (selected.Count == 0)
            {
                System.Windows.MessageBox.Show(_localization["SelectBackupToExecute"], _localization["Error"], MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var names = selected.Select(b => b.BackupName).ToList();
            var results = _backupService.ExecuteBackups(names);

            if(results != null)
            {
                System.Windows.MessageBox.Show(_localization["ExecutionResult"], _localization["Success"], MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadBackups();
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

            IsSettingsDialogOpen = false;
            OnPropertyChanged(nameof(IsSettingsDialogOpen));
            System.Windows.MessageBox.Show("Paramètres enregistrés avec succès.");
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}