// ============================================================================
//  Project:        Deployment.Tools.DeterministicSerializer
//  File:           Program.cs
//  Created:        2026-08-10
//  Description:    CI gating tool. Deserializes one manifest JSON file
//                  (autodetected the same explicit, deterministic way
//                  ManifestValidator does) into its real contract record and
//                  re-serializes it with fixed JsonSerializerOptions, writing
//                  the result to stdout. A CI job runs this twice against the
//                  same input and diffs the two outputs -- since every
//                  Deployment contract is an immutable record with
//                  compile-time-ordered properties (no Dictionary-valued
//                  fields), the two runs must byte-for-byte match; a
//                  divergence would mean a future contract change introduced
//                  non-deterministic shape (e.g. an unordered collection),
//                  which this check exists to catch before it reaches a
//                  ConfigSnapshot hash.
// ============================================================================
using System.Text.Json;
using Deployment.ConfigSchema.Contracts;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: DeterministicSerializer <manifest.json>");
    return 1;
}

string path = args[0];
if (!File.Exists(path))
{
    Console.Error.WriteLine($"File not found: {path}");
    return 1;
}

string json = File.ReadAllText(path);
var readOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var writeOptions = new JsonSerializerOptions { WriteIndented = true };

try
{
    using JsonDocument document = JsonDocument.Parse(json);
    JsonElement root = document.RootElement;

    object model;
    Type modelType;

    if (root.TryGetProperty("strategy", out _))
    {
        modelType = typeof(DeploymentConfig);
        model = JsonSerializer.Deserialize<DeploymentConfig>(json, readOptions)
            ?? throw new InvalidOperationException("Deserialized to null.");
    }
    else if (root.TryGetProperty("topology", out _))
    {
        modelType = typeof(TenantConfig);
        model = JsonSerializer.Deserialize<TenantConfig>(json, readOptions)
            ?? throw new InvalidOperationException("Deserialized to null.");
    }
    else if (root.TryGetProperty("roleId", out _))
    {
        modelType = typeof(RoleConfig);
        model = JsonSerializer.Deserialize<RoleConfig>(json, readOptions)
            ?? throw new InvalidOperationException("Deserialized to null.");
    }
    else if (root.TryGetProperty("moduleId", out _) && root.TryGetProperty("state", out _))
    {
        modelType = typeof(ModuleConfig);
        model = JsonSerializer.Deserialize<ModuleConfig>(json, readOptions)
            ?? throw new InvalidOperationException("Deserialized to null.");
    }
    else
    {
        Console.Error.WriteLine($"[UNKNOWN]  {path}: does not match a DeploymentConfig/TenantConfig/RoleConfig/ModuleConfig shape.");
        return 1;
    }

    Console.WriteLine(JsonSerializer.Serialize(model, modelType, writeOptions));
    return 0;
}
catch (JsonException ex)
{
    Console.Error.WriteLine($"[PARSE ERROR] {path}: {ex.Message}");
    return 1;
}
