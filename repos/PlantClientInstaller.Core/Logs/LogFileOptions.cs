namespace PlantClientInstaller.Core.Logs;

public sealed class LogFileOptions
{
    public string DirectoryPath { get; }
    public string FileName { get; }
    public int MaxFileSizeKb { get; }

    public LogFileOptions(string directoryPath, string fileName = "installer.log", int maxFileSizeKb = 1024)
    {
        DirectoryPath = directoryPath;
        FileName = fileName;
        MaxFileSizeKb = maxFileSizeKb;
    }

    public string FullPath => Path.Combine(DirectoryPath, FileName);
}
