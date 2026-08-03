namespace PlantClientInstaller.Core.Payload.Assets;

public interface IAssetProvider
{
    IReadOnlyList<AssetModel> LoadAll();
    bool TryLoadAll(out IReadOnlyList<AssetModel>? assets);
    AssetModel Get(string assetName);
}
