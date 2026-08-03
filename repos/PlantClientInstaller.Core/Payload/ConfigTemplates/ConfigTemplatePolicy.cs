namespace PlantClientInstaller.Core.Payload.ConfigTemplates;

public sealed class ConfigTemplatePolicy
{
    public bool RequireVersion { get; }
    public bool RequireFileExists { get; }

    public ConfigTemplatePolicy(
        bool requireVersion = true,
        bool requireFileExists = true)
    {
        RequireVersion = requireVersion;
        RequireFileExists = requireFileExists;
    }
}
