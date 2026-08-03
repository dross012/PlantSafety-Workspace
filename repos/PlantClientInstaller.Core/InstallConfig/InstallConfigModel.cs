namespace PlantClientInstaller.Core.InstallConfig;

public sealed class InstallConfigModel
{
    public string PlantId { get; }
    public string InstallerId { get; }
    public string TargetDirectory { get; }
    public bool EnableDiagnostics { get; }
    public bool EnableTelemetry { get; }

    public InstallConfigModel(
        string plantId,
        string installerId,
        string targetDirectory,
        bool enableDiagnostics,
        bool enableTelemetry)
    {
        PlantId = plantId;
        InstallerId = installerId;
        TargetDirectory = targetDirectory;
        EnableDiagnostics = enableDiagnostics;
        EnableTelemetry = enableTelemetry;
    }
}
