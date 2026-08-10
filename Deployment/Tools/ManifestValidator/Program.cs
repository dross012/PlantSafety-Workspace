// ============================================================================
//  Project:        Deployment.Tools.ManifestValidator
//  File:           Program.cs
//  Created:        2026-08-10
//  Description:    CI gating tool. Deserializes one or more manifest JSON
//                  files (DeploymentConfig/TenantConfig/RoleConfig/
//                  ModuleConfig shape, autodetected by an explicit,
//                  deterministic property check -- never reflection) and
//                  runs them through the real Deployment.ConfigSchema.
//                  Registry.ConfigValidator / Deployment.Boundaries.
//                  RoleValidator pipeline already shipped in Phase 18.
//                  Exits 0 if every manifest is valid, 1 otherwise, so it
//                  can gate a CI job directly.
// ============================================================================
using System.Text.Json;
using Deployment.Boundaries;
using Deployment.ConfigSchema.Contracts;
using Deployment.ConfigSchema.Registry;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: ManifestValidator <manifest.json> [<manifest.json> ...]");
    return 1;
}

var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
bool anyInvalid = false;

foreach (string path in args)
{
    if (!File.Exists(path))
    {
        Console.Error.WriteLine($"[MISSING]  {path}");
        anyInvalid = true;
        continue;
    }

    string json = File.ReadAllText(path);

    IReadOnlyList<string> violations;
    string manifestType;

    try
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        if (root.TryGetProperty("strategy", out _))
        {
            manifestType = nameof(DeploymentConfig);
            DeploymentConfig config = JsonSerializer.Deserialize<DeploymentConfig>(json, jsonOptions)
                ?? throw new InvalidOperationException("Deserialized to null.");
            violations = ConfigValidator.ValidateDeploymentConfig(config);
        }
        else if (root.TryGetProperty("topology", out _))
        {
            manifestType = nameof(TenantConfig);
            TenantConfig config = JsonSerializer.Deserialize<TenantConfig>(json, jsonOptions)
                ?? throw new InvalidOperationException("Deserialized to null.");
            violations = ConfigValidator.ValidateTenantConfig(config);
        }
        else if (root.TryGetProperty("roleId", out _))
        {
            manifestType = nameof(RoleConfig);
            RoleConfig config = JsonSerializer.Deserialize<RoleConfig>(json, jsonOptions)
                ?? throw new InvalidOperationException("Deserialized to null.");
            violations = RoleValidator.ValidateRoleCatalog(
                new Dictionary<string, RoleConfig>(StringComparer.Ordinal) { [config.RoleId] = config });
        }
        else if (root.TryGetProperty("moduleId", out _) && root.TryGetProperty("state", out _))
        {
            manifestType = nameof(ModuleConfig);
            ModuleConfig config = JsonSerializer.Deserialize<ModuleConfig>(json, jsonOptions)
                ?? throw new InvalidOperationException("Deserialized to null.");
            violations = ConfigValidator.ValidateModuleConfig(config);
        }
        else
        {
            Console.Error.WriteLine($"[UNKNOWN]  {path}: does not match a DeploymentConfig/TenantConfig/RoleConfig/ModuleConfig shape.");
            anyInvalid = true;
            continue;
        }
    }
    catch (JsonException ex)
    {
        Console.Error.WriteLine($"[PARSE ERROR] {path}: {ex.Message}");
        anyInvalid = true;
        continue;
    }

    if (violations.Count == 0)
    {
        Console.WriteLine($"[VALID]    {path} ({manifestType})");
    }
    else
    {
        anyInvalid = true;
        Console.WriteLine($"[INVALID]  {path} ({manifestType}) -- {violations.Count} violation(s):");
        foreach (string violation in violations)
        {
            Console.WriteLine($"             - {violation}");
        }
    }
}

return anyInvalid ? 1 : 0;
