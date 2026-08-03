namespace PlantClientInstaller.Core.Payload.ClientApp;

public sealed class ClientAppModel
{
    public string Name { get; }
    public string ExecutablePath { get; }
    public string Version { get; }
    public IReadOnlyList<string> Dependencies { get; }

    public ClientAppModel(
        string name,
        string executablePath,
        string version,
        IReadOnlyList<string> dependencies)
    {
        Name = name;
        ExecutablePath = executablePath;
        Version = version;
        Dependencies = dependencies;
    }
}
