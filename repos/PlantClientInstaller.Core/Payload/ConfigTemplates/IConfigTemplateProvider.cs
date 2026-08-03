namespace PlantClientInstaller.Core.Payload.ConfigTemplates;

public interface IConfigTemplateProvider
{
    IReadOnlyList<ConfigTemplateModel> LoadAll();
    bool TryLoadAll(out IReadOnlyList<ConfigTemplateModel>? templates);
    ConfigTemplateModel Get(string templateName);
}
