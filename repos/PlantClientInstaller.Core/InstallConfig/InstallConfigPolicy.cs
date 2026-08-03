namespace PlantClientInstaller.Core.InstallConfig;

public sealed class InstallConfigPolicy
{
    public bool AllowMissingTelemetryFlag { get; }
    public bool AllowMissingDiagnosticsFlag { get; }

    public InstallConfigPolicy(
        bool allowMissingTelemetryFlag = false,
        bool allowMissingDiagnosticsFlag = false)
    {
        AllowMissingTelemetryFlag = allowMissingTelemetryFlag;
        AllowMissingDiagnosticsFlag = allowMissingDiagnosticsFlag;
    }
}
