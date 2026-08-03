namespace PlantClientInstaller.Core.Payload.ClientApp;

public sealed class ClientAppManifest
{
    public string Name { get; set; } = "";
    public string Executable { get; set; } = "";
    public string Version { get; set; } = "";
    public List<string> Dependencies { get; set; } = new();
}
