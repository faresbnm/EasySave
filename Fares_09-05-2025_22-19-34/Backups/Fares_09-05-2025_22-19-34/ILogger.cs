namespace EasySave.Logging
{
    public interface ILogger
    {
        void LogTransfer(string backupName, string sourcePath, string targetPath, long fileSize, double transferTimeMs);
    }
}