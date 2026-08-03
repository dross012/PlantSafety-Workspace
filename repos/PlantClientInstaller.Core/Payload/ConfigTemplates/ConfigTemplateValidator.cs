namespace PlantClientInstaller.Core.Payload.ConfigTemplates;

public sealed class ConfigTemplateValidator
{
    public void Validate(ConfigTemplateManifest manifest, ConfigTemplatePolicy policy, string templateDirectory)
    {
        if (manifest.Entries.Count == 0)
            throw new InvalidOperationException("ConfigTemplate manifest contains no entries.");

        foreach (var entry in manifest.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Name))
                throw new InvalidOperationException("ConfigTemplate entry missing Name.");

            if (policy.RequireVersion && string.IsNullOrWhiteSpace(entry.Version))
                throw new InvalidOperationException($"ConfigTemplate '{entry.Name}' missing Version.");

            var filePath = Path.Combine(templateDirectory, entry.File);

            if (policy.RequireFileExists && !File.Exists(filePath))
                throw new FileNotFoundException($"ConfigTemplate file missing: {filePath}");
        }
    }
}
