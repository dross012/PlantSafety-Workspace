namespace PlantClientInstaller.Core.Payload.ClientApp;

public static class ClientAppExtensions
{
    public static bool IsDotNetApp(this ClientAppModel model) =>
        model.ExecutablePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
        model.ExecutablePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);

    public static bool HasDependencies(this ClientAppModel model) =>
        model.Dependencies.Count > 0;
}
