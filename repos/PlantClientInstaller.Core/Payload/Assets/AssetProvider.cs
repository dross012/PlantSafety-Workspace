namespace PlantClientInstaller.Core.Payload.Assets;

public sealed class AssetProvider : IAssetProvider
{
    private readonly AssetManifestLoader _loader;
    private readonly AssetValidator _validator;
    private readonly AssetPolicy _policy;
    private readonly AssetOptions _options;

    public AssetProvider(
        AssetManifestLoader loader,
        AssetValidator validator,
        AssetPolicy policy,
        AssetOptions options)
    {
        _loader = loader;
        _validator = validator;
        _policy = policy;
        _options = options;
    }

    public IReadOnlyList<AssetModel> LoadAll()
    {
        var manifest = _loader.Load();
        _validator.ValidateManifest(manifest, _policy);

        var list = new List<AssetModel>();

        foreach (var entry in manifest.Entries)
        {
            var filePath = Path.Combine(_options.AssetDirectory, entry.File);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Asset file missing: {filePath}");

            var size = new FileInfo(filePath).Length;

            var checksum = new AssetChecksum(entry.ChecksumAlgorithm, entry.ChecksumValue);

            _validator.ValidateChecksum(filePath, checksum, _policy);

            list.Add(new AssetModel(entry.Name, filePath, size, checksum));
        }

        return list;
    }

    public bool TryLoadAll(out IReadOnlyList<AssetModel>? assets)
    {
        try
        {
            assets = LoadAll();
            return true;
        }
        catch
        {
            assets = null;
            return false;
        }
    }

    public AssetModel Get(string assetName)
    {
        var all = LoadAll();
        return all.FirstOrDefault(a => a.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase))
               ?? throw new KeyNotFoundException($"Asset '{assetName}' not found.");
    }
}
