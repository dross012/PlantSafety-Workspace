namespace PlantClientInstaller.Core.Payload.ClientApp;

public sealed class ClientAppOptions
{
    public string AppDirectory { get; }
    public string ManifestFileName { get; }

    public ClientAppOptions(string appDirectory, string manifestFileName = "clientapp.manifest.json")
    {
        AppDirectory = appDirectory;
        ManifestFileName = manifestFileName;
    }

    public string ManifestPath => Path.Combine(AppDirectory, ManifestFileName);
}
