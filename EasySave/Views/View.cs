using System;
using System.Collections.Generic;
using EasySave.Localization;
using EasySave.Model;

namespace EasySave.Views
{
    internal class View
    {
        private readonly ILocalizationService _localization;

        public View(ILocalizationService localization)
        {
            _localization = localization;
        }

        public void ShowStart()
        {
            Console.Clear();
            Console.WriteLine(_localization["WelcomeTitle"]);
            Console.WriteLine("\n");
            Console.WriteLine(_localization["MenuTitle"]);
            Console.WriteLine(_localization["MenuOption1"]);
            Console.WriteLine(_localization["MenuOption2"]);
            Console.WriteLine(_localization["MenuOption3"]);
            Console.WriteLine(_localization["MenuOption4"]);
            Console.WriteLine(_localization["MenuOption5"]);
            Console.WriteLine(_localization["MenuOption6"]);
            Console.Write("\n" + _localization["SelectOption"]);
        }

        public (string name, string source, string target, int type) GetBackupInfo()
        {
            Console.Clear();
            Console.WriteLine(_localization["NewBackupTitle"]);
            string name = GetUserInput(_localization["EnterBackupName"]);
            string source = GetUserInput(_localization["EnterSourceDirectory"]);
            string target = GetUserInput(_localization["EnterTargetDirectory"]);

            int type;
            while (true)
            {
                string typeInput = GetUserInput(_localization["EnterBackupType"]);
                if (int.TryParse(typeInput, out type) && (type == 1 || type == 2))
                {
                    break;
                }
                ShowError(_localization["InvalidBackupType"]);
            }
            return (name, source, target, type);
        }

        public string GetUserInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        public void ShowMessage(string messageKey)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(_localization[messageKey]);
            Console.ResetColor();
            Console.WriteLine(_localization["PressAnyKeyToContinue"]);
            Console.ReadKey();
        }

        public void ShowError(string messageKey)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(_localization[messageKey]);
            Console.ResetColor();
            Console.WriteLine(_localization["PressAnyKeyToContinue"]);
            Console.ReadKey();
        }

        public void ListBackups(List<Backup> backups)
        {
            Console.Clear();
            Console.WriteLine(_localization["BackupJobsList"]);

            if (backups.Count == 0)
            {
                Console.WriteLine(_localization["NoBackupJobs"]);
            }
            else
            {
                foreach (var backup in backups)
                {
                    Console.WriteLine(_localization.Format("BackupDetails",
                        backup.BackupName,
                        backup.Source,
                        backup.Target,
                        backup.Type == 1 ? _localization["BackupTypeFull"] : _localization["BackupTypeDifferential"]));
                    Console.WriteLine("-----------------------");
                }
            }

            Console.WriteLine("\n" + _localization["PressAnyKeyToContinue"]);
            Console.ReadKey();
        }

        public string GetBackupNameToDelete()
        {
            Console.Clear();
            Console.WriteLine(_localization["DeleteBackupTitle"]);
            return GetUserInput(_localization["EnterBackupToDelete"]);
        }

        public string GetBackupNameToUpdate()
        {
            Console.Clear();
            Console.WriteLine(_localization["UpdateBackupTitle"]);
            return GetUserInput(_localization["EnterBackupToUpdate"]);
        }

        public Backup GetUpdatedBackupInfo(Backup currentBackup)
        {
            Console.Clear();
            Console.WriteLine(_localization["UpdateBackupTitle"]);

            string newName = GetUserInput(_localization.Format("NamePrompt", currentBackup.BackupName));
            string newSource = GetUserInput(_localization.Format("SourcePrompt", currentBackup.Source));
            string newTarget = GetUserInput(_localization.Format("TargetPrompt", currentBackup.Target));

            int newType;
            while (true)
            {
                string typeInput = GetUserInput(_localization.Format("TypePrompt",
                    currentBackup.Type == 1 ? "1 (" + _localization["BackupTypeFull"] + ")" : "2 (" + _localization["BackupTypeDifferential"] + ")"));

                if (string.IsNullOrEmpty(typeInput))
                {
                    newType = currentBackup.Type;
                    break;
                }
                if (int.TryParse(typeInput, out newType) && (newType == 1 || newType == 2))
                {
                    break;
                }
                ShowError(_localization["InvalidBackupType"]);
            }

            return new Backup(
                string.IsNullOrEmpty(newName) ? currentBackup.BackupName : newName,
                string.IsNullOrEmpty(newSource) ? currentBackup.Source : newSource,
                string.IsNullOrEmpty(newTarget) ? currentBackup.Target : newTarget,
                newType
            );
        }

        public void ShowCurrentBackup(Backup backup)
        {
            Console.WriteLine(_localization["CurrentBackupDetails"]);
            Console.WriteLine(_localization.Format("BackupDetailName", backup.BackupName));
            Console.WriteLine(_localization.Format("BackupDetailSource", backup.Source));
            Console.WriteLine(_localization.Format("BackupDetailTarget", backup.Target));
            Console.WriteLine(_localization.Format("BackupDetailType",
                backup.Type == 1 ? _localization["BackupTypeFull"] : _localization["BackupTypeDifferential"]));
            Console.WriteLine("-----------------------\n");
        }

        public string GetExecutionChoice()
        {
            Console.Clear();
            Console.WriteLine(_localization["BackupExecutionOptions"]);
            Console.WriteLine(_localization["ExecutionOption1"]);
            Console.WriteLine(_localization["ExecutionOption2"]);
            Console.WriteLine(_localization["ExecutionOption3"]);
            Console.WriteLine(_localization["ExecutionOption4"]);
            return GetUserInput(_localization["SelectExecutionOption"]);
        }

        public string GetBackupNameToExecute()
        {
            Console.Clear();
            Console.WriteLine(_localization["ExecuteSingleBackup"]);
            return GetUserInput(_localization["EnterBackupToExecute"]);
        }

        public void ShowBackupInProgress(string backupName, string backupType)
        {
            Console.Clear();
            Console.WriteLine(_localization.Format("ExecutingBackup",
                backupType == "1" ? _localization["BackupTypeFull"] : _localization["BackupTypeDifferential"],
                backupName));
            Console.WriteLine(_localization["PleaseWait"]);
        }
    }
}