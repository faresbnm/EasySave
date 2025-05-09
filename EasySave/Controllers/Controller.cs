using System;
using System.Collections.Generic;
using System.IO;
using EasySave.Model;
using EasySave.Views;

namespace EasySave.Controllers
{
    internal class Controller
    {
        private readonly View _view;
        private readonly BackupService _backupService;

        public Controller()
        {
            _view = new View();
            _backupService = new BackupService(); 
            Run();
        }

        private void Run()
        {
            while (true)
            {
                _view.ShowStart();
                var input = _view.GetUserInput("");

                switch (input)
                {
                    case "1":
                        ListBackups();
                        break;
                    case "2":
                        AddBackup();
                        break;
                    case "3":
                        UpdateBackup();
                        break;
                    case "4":
                        DeleteBackup();
                        break;
                    case "5":
                        ExecuteBackup();
                        break;
                    case "6":
                        Environment.Exit(0);
                        break;
                    default:
                        _view.ShowMessage("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private void ListBackups()
        {
            var backups = _backupService.GetAllBackups();
            _view.ListBackups(backups);
        }

        private void AddBackup()
        {
            var (name, source, target, type) = _view.GetBackupInfo();
            var newBackup = new Backup(name, source, target, type);

            // Validate the backup
            var (isValid, message) = _backupService.ValidateBackup(newBackup);

            if (!isValid)
            {
                _view.ShowError(message);
                return;
            }

            // If valid, create the backup
            var result = _backupService.CreateBackup(newBackup);
            _view.ShowMessage(result);
        }

        private void DeleteBackup()
        {
            string backupName = _view.GetBackupNameToDelete();
            string result = _backupService.DeleteBackup(backupName);

            bool isSuccess = !result.Contains("not found") && !result.Contains("Failed");
            if (!isSuccess)
            {
                _view.ShowError(result);
            }
            else
            {
                _view.ShowMessage(result);
            }
        }

        private void UpdateBackup()
        {
            string backupName = _view.GetBackupNameToUpdate();
            var (backup, message) = _backupService.GetBackupByName(backupName);

            if (backup == null)
            {
                _view.ShowError(message);
                return;
            }

            _view.ShowCurrentBackup(backup);
            Backup updatedBackup = _view.GetUpdatedBackupInfo(backup);

            var (isValid, message2) = _backupService.ValidateUpdatedBackup(updatedBackup);
            if (!isValid)
            {
                _view.ShowError(message2);
                return;
            }


            string result = _backupService.UpdateBackup(backupName, updatedBackup);
            bool isSuccess = !result.Contains("not found") && !result.Contains("Failed");
            if (!isSuccess)
            {
                _view.ShowError(result);
            }
            else
            {
                _view.ShowMessage(result);
            }
        }

        private void ExecuteBackup()
        {
            var backups = _backupService.GetAllBackups();
            if (backups.Count == 0)
            {
                _view.ShowError("No backups available");
                return;
            }

            _view.ListBackups(backups);
            string input = _view.GetExecutionChoice();

            var (backupNames, error) = _backupService.ParseBackupSelection(input, backups);
            if (error != null)
            {
                _view.ShowError(error);
                return;
            }

            var results = _backupService.ExecuteBackups(backupNames);

            foreach (var result in results)
            {
                if (result.Contains("Failed") || result.Contains("not found"))
                    _view.ShowError(result);
                else
                    _view.ShowMessage(result);
            }
        }

    }
}
