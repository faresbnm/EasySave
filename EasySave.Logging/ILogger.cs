namespace EasySave.Logging
{
    public interface ILogger
    {
        void LogTransfer(string backupName, string sourcePath, string targetPath, long fileSize, double transferTimeMs);
        LogFormat Format { get; set; }

    }
    public enum LogFormat
    {
        Json,
        Xml
    }
}