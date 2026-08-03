namespace PlantClientInstaller.Core.Payload.ConfigTemplates;

public sealed class ConfigTemplateRenderer
{
    public string Render(ConfigTemplateModel template, IReadOnlyDictionary<string, string> tokens)
    {
        var content = File.ReadAllText(template.FilePath);

        foreach (var kvp in tokens)
        {
            content = content.Replace($"{{{{{kvp.Key}}}}}", kvp.Value, StringComparison.OrdinalIgnoreCase);
        }

        return content;
    }
}
