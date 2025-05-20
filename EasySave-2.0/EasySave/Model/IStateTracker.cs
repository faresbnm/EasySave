namespace EasySave.Logging
{
    public interface IStateTracker
    {
        void InitializeState(string backupName);
        void UpdateState(string backupName, string status, int totalFiles = 0, long totalSize = 0,
                         int filesCopied = 0, long sizeCopied = 0,
                         string currentSource = null, string currentTarget = null);
        void ClearState(string backupName);
    }
}