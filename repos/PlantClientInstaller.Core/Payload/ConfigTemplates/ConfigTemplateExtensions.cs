namespace PlantClientInstaller.Core.Payload.ConfigTemplates;

public static class ConfigTemplateExtensions
{
    public static bool IsJsonTemplate(this ConfigTemplateModel model) =>
        model.FilePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

    public static bool IsXmlTemplate(this ConfigTemplateModel model) =>
        model.FilePath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

    public static bool IsIniTemplate(this ConfigTemplateModel model) =>
        model.FilePath.EndsWith(".ini", StringComparison.OrdinalIgnoreCase);
}
