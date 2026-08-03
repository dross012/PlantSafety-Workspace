namespace PlantClientInstaller.Core.InstallConfig;

public static class InstallConfigExtensions
{
    public static bool IsTelemetryEnabled(this InstallConfigModel model) =>
        model.EnableTelemetry;

    public static bool IsDiagnosticsEnabled(this InstallConfigModel model) =>
        model.EnableDiagnostics;
}
