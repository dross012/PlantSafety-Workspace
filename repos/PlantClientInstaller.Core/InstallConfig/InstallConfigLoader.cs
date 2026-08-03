using System.Text.Json;

namespace PlantClientInstaller.Core.InstallConfig;

public sealed class InstallConfigLoader
{
    private readonly InstallConfigOptions _options;

    public InstallConfigLoader(InstallConfigOptions options)
    {
        _options = options;
    }

    public InstallConfigModel Load()
    {
        var path = _options.FullPath;

        if (!File.Exists(path))
            throw new FileNotFoundException($"Installer config file not found: {path}");

        var json = File.ReadAllText(path);
        var dto = JsonSerializer.Deserialize<InstallConfigDto>(json)
                  ?? throw new InvalidOperationException("Invalid installer config JSON.");

        return new InstallConfigModel(
            dto.PlantId,
            dto.InstallerId,
            dto.TargetDirectory,
            dto.EnableDiagnostics,
            dto.EnableTelemetry
        );
    }

    private sealed class InstallConfigDto
    {
        public string PlantId { get; set; } = "";
        public string InstallerId { get; set; } = "";
        public string TargetDirectory { get; set; } = "";
        public bool EnableDiagnostics { get; set; }
        public bool EnableTelemetry { get; set; }
    }
}
