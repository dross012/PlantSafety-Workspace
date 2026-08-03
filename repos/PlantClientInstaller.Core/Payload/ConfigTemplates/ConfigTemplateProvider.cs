namespace PlantClientInstaller.Core.Payload.ConfigTemplates;

public sealed class ConfigTemplateProvider : IConfigTemplateProvider
{
    private readonly ConfigTemplateManifestLoader _loader;
    private readonly ConfigTemplateValidator _validator;
    private readonly ConfigTemplatePolicy _policy;
    private readonly ConfigTemplateOptions _options;

    public ConfigTemplateProvider(
        ConfigTemplateManifestLoader loader,
        ConfigTemplateValidator validator,
        ConfigTemplatePolicy policy,
        ConfigTemplateOptions options)
    {
        _loader = loader;
        _validator = validator;
        _policy = policy;
        _options = options;
    }

    public IReadOnlyList<ConfigTemplateModel> LoadAll()
    {
        var manifest = _loader.Load();
        _validator.Validate(manifest, _policy, _options.TemplateDirectory);

        var list = new List<ConfigTemplateModel>();

        foreach (var entry in manifest.Entries)
        {
            var filePath = Path.Combine(_options.TemplateDirectory, entry.File);

            list.Add(new ConfigTemplateModel(
                entry.Name,
                filePath,
                entry.Version
            ));
        }

        return list;
    }

    public bool TryLoadAll(out IReadOnlyList<ConfigTemplateModel>? templates)
    {
        try
        {
            templates = LoadAll();
            return true;
        }
        catch
        {
            templates = null;
            return false;
        }
    }

    public ConfigTemplateModel Get(string templateName)
    {
        var all = LoadAll();
        return all.FirstOrDefault(t => t.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase))
               ?? throw new KeyNotFoundException($"ConfigTemplate '{templateName}' not found.");
    }
}
