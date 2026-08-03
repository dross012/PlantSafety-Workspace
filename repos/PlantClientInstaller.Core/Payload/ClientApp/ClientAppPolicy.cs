namespace PlantClientInstaller.Core.Payload.ClientApp;

public sealed class ClientAppPolicy
{
    public bool RequireExecutableExists { get; }
    public bool RequireVersion { get; }
    public bool RequireDependenciesExist { get; }

    public ClientAppPolicy(
        bool requireExecutableExists = true,
        bool requireVersion = true,
        bool requireDependenciesExist = true)
    {
        RequireExecutableExists = requireExecutableExists;
        RequireVersion = requireVersion;
        RequireDependenciesExist = requireDependenciesExist;
    }
}
