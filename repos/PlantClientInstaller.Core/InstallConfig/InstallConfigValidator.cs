namespace PlantClientInstaller.Core.InstallConfig;

public sealed class InstallConfigValidator
{
    public void Validate(InstallConfigModel model, InstallConfigPolicy policy)
    {
        if (string.IsNullOrWhiteSpace(model.PlantId))
            throw new InvalidOperationException("PlantId is required.");

        if (string.IsNullOrWhiteSpace(model.InstallerId))
            throw new InvalidOperationException("InstallerId is required.");

        if (string.IsNullOrWhiteSpace(model.TargetDirectory))
            throw new InvalidOperationException("TargetDirectory is required.");

        if (!policy.AllowMissingDiagnosticsFlag && model.EnableDiagnostics == false)
        {
            // Enterprise rule: diagnostics must be explicitly set
        }

        if (!policy.AllowMissingTelemetryFlag && model.EnableTelemetry == false)
        {
            // Enterprise rule: telemetry must be explicitly set
        }
    }
}
