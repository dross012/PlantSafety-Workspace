namespace PlantClientInstaller.Core.Logs;

public sealed class LogFileProvider
{
    private readonly LogFileOptions _options;

    public LogFileProvider(LogFileOptions options)
    {
        _options = options;
        Directory.CreateDirectory(_options.DirectoryPath);
    }

    public string GetPath() => _options.FullPath;

    public void RotateIfNeeded()
    {
        var file = _options.FullPath;
        if (!File.Exists(file)) return;

        var sizeKb = new FileInfo(file).Length / 1024;
        if (sizeKb < _options.MaxFileSizeKb) return;

        var rotated = $"{file}.{DateTime.UtcNow:yyyyMMddHHmmss}.bak";
        File.Move(file, rotated, overwrite: true);
    }
}
