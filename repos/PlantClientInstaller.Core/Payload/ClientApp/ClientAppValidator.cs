namespace PlantClientInstaller.Core.Payload.ClientApp;

public sealed class ClientAppValidator
{
    public void Validate(ClientAppManifest manifest, ClientAppPolicy policy, string appDirectory)
    {
        if (string.IsNullOrWhiteSpace(manifest.Name))
            throw new InvalidOperationException("ClientApp manifest missing Name.");

        if (policy.RequireVersion && string.IsNullOrWhiteSpace(manifest.Version))
            throw new InvalidOperationException("ClientApp manifest missing Version.");

        var exePath = Path.Combine(appDirectory, manifest.Executable);

        if (policy.RequireExecutableExists && !File.Exists(exePath))
            throw new FileNotFoundException($"ClientApp executable not found: {exePath}");

        if (policy.RequireDependenciesExist)
        {
            foreach (var dep in manifest.Dependencies)
            {
                var depPath = Path.Combine(appDirectory, dep);
                if (!File.Exists(depPath))
                    throw new FileNotFoundException($"ClientApp dependency missing: {depPath}");
            }
        }
    }
}
