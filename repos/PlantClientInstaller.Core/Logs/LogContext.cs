namespace PlantClientInstaller.Core.Logs;

public sealed class LogContext
{
    public string InstallerId { get; }
    public string MachineName { get; }
    public string? Additional { get; }

    public LogContext(string installerId, string machineName, string? additional = null)
    {
        InstallerId = installerId;
        MachineName = machineName;
        Additional = additional;
    }

    public override string ToString() =>
        $"{InstallerId}:{MachineName}" + (Additional is not null ? $"[{Additional}]" : "");
}
