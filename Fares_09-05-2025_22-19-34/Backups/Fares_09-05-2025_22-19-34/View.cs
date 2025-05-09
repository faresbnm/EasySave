using System;
using System.Collections.Generic;
using EasySave.Model;

namespace EasySave.Views
{
    internal class View
    {
        public void ShowStart()
        {
            Console.Clear();
            Console.WriteLine("  ______     ______     ______     __  __        ______     ______     __   __   ______       \r\n/\\  ___\\   /\\  __ \\   /\\  ___\\   /\\ \\_\\ \\      /\\  ___\\   /\\  __ \\   /\\ \\ / /  /\\  ___\\      \r\n\\ \\  __\\   \\ \\  __ \\  \\ \\___  \\  \\ \\____ \\     \\ \\___  \\  \\ \\  __ \\  \\ \\ \\'/   \\ \\  __\\      \r\n \\ \\_____\\  \\ \\_\\ \\_\\  \\/\\_____\\  \\/\\_____\\     \\/\\_____\\  \\ \\_\\ \\_\\  \\ \\__|    \\ \\_____\\    \r\n  \\/_____/   \\/_/\\/_/   \\/_____/   \\/_____/      \\/_____/   \\/_/\\/_/   \\/_/      \\/_____/    \r\n                                                                                             ");
            Console.WriteLine("\n");
            Console.WriteLine("[+] Menu:");
            Console.WriteLine("1- List Backup jobs");
            Console.WriteLine("2- Add a job");
            Console.WriteLine("3- Update a job");
            Console.WriteLine("4- Delete a job");
            Console.WriteLine("5- Execute a job");
            Console.WriteLine("6- Exit");
            Console.Write("\nSelect an option: ");
        }

        public (string name, string source, string target, int type) GetBackupInfo()
        {
            Console.Clear();
            Console.WriteLine("[+] New Backup");
            string name = GetUserInput("Enter the backup name: ");
            string source = GetUserInput("Enter the source directory: ");
            string target = GetUserInput("Enter the target directory: ");

            int type;
            while (true)
            {
                string typeInput = GetUserInput("Enter the type of backup (1 for full, 2 for differential): ");
                if (int.TryParse(typeInput, out type))
                {
                    break;
                }
            }
            return (name, source, target, type);
        }

        public string GetUserInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        public void ShowMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[+] {message}");
            Console.ResetColor(); 
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[!] {message}");
            Console.ResetColor();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public void ListBackups(List<Model.Backup> backups)
        {
            Console.Clear();
            Console.WriteLine("[+] Backup Jobs List:");

            if (backups.Count == 0)
            {
                Console.WriteLine("No backup jobs found.");
            }
            else
            {
                foreach (var backup in backups)
                {
                    Console.WriteLine($"\nName: {backup.BackupName}");
                    Console.WriteLine($"Source: {backup.Source}");
                    Console.WriteLine($"Target: {backup.Target}");
                    Console.WriteLine($"Type: {(backup.Type == 1 ? "Full" : "Differential")}");
                    Console.WriteLine("-----------------------");
                }
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public string GetBackupNameToDelete()
        {
            Console.Clear();
            Console.WriteLine("[+] Delete Backup");
            return GetUserInput("Enter the name of the backup to delete: ");
        }

        public string GetBackupNameToUpdate()
        {
            Console.Clear();
            Console.WriteLine("[+] Update Backup");
            return GetUserInput("Enter the name of the backup to update: ");
        }

        public Backup GetUpdatedBackupInfo(Backup currentBackup)
        {
            Console.Clear();
            Console.WriteLine("[+] Update Backup");

            string newName = GetUserInput($"Name [{currentBackup.BackupName}]: ");
            string newSource = GetUserInput($"Source [{currentBackup.Source}]: ");
            string newTarget = GetUserInput($"Target [{currentBackup.Target}]: ");

            int newType;
            while (true)
            {
                string typeInput = GetUserInput($"Type [{(currentBackup.Type == 1 ? "1 (Full)" : "2 (Differential)")}]: ");
                if (string.IsNullOrEmpty(typeInput))
                {
                    newType = currentBackup.Type;
                    break;
                }
                if (int.TryParse(typeInput, out newType) && (newType == 1 || newType == 2))
                {
                    break;
                }
                Console.WriteLine("Invalid backup type. Please enter 1 or 2.");
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
            Console.WriteLine("\nCurrent Backup Details:");
            Console.WriteLine($"Name: {backup.BackupName}");
            Console.WriteLine($"Source: {backup.Source}");
            Console.WriteLine($"Target: {backup.Target}");
            Console.WriteLine($"Type: {(backup.Type == 1 ? "Full" : "Differential")}");
            Console.WriteLine("-----------------------\n");
        }

        public string GetExecutionChoice()
        {
            Console.Clear();
            Console.WriteLine("[+] Backup Execution Options:");
            Console.WriteLine("- Enter backup number (e.g., '1')");
            Console.WriteLine("- Comma-separated list (e.g., '1,3,5')");
            Console.WriteLine("- Range (e.g., '2-4')");
            Console.WriteLine("- 'all' for all backups");
            return GetUserInput("Select an option: ");
        }

        public string GetBackupNameToExecute()
        {
            Console.Clear();
            Console.WriteLine("[+] Execute Single Backup");
            return GetUserInput("Enter the name of the backup to execute: ");
        }

        public void ShowBackupInProgress(string backupName, string backupType)
        {
            Console.Clear();
            Console.WriteLine($"[+] Executing {backupType} backup: {backupName}");
            Console.WriteLine("Please wait...");
        }
    }
}
