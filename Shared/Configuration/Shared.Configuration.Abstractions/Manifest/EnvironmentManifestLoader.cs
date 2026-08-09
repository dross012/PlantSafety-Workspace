// ============================================================================
//  Project:        Shared.Configuration
//  Module:         Abstractions.Manifest
//  File:           EnvironmentManifestLoader.cs
//  Created:        2026-08-09
//  Description:    Phase 2.5 deliverable: the unified configuration
//                  loader. The one place an EnvironmentManifest is built
//                  from IConfiguration -- no module reads
//                  IConfiguration["Environment:..."] directly. Missing
//                  sections default to empty (not an error by themselves;
//                  EnvironmentManifestValidator decides what's actually
//                  required).
// ============================================================================
using Microsoft.Extensions.Configuration;

namespace Shared.Configuration.Abstractions.Manifest;

/// <summary>Builds an EnvironmentManifest from IConfiguration's "Environment" section.</summary>
public static class EnvironmentManifestLoader
{
    /// <summary>Loads the environment manifest from configuration.</summary>
    public static EnvironmentManifest Load(IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection("Environment");

        var featureFlags = new Dictionary<string, bool>();
        foreach (IConfigurationSection flag in section.GetSection("FeatureFlags").GetChildren())
        {
            featureFlags[flag.Key] = flag.Get<bool>();
        }

        var connectionStrings = new Dictionary<string, string>();
        foreach (IConfigurationSection entry in section.GetSection("ConnectionStrings").GetChildren())
        {
            connectionStrings[entry.Key] = entry.Value ?? string.Empty;
        }

        // Secrets are read the same way as any other configuration value (appsettings,
        // environment variables, a real secret store in production) -- never hardcoded,
        // never logged. EnvironmentManifest itself carries no ToString override that
        // would risk a secret leaking into a log line via string interpolation.
        var secrets = new Dictionary<string, string>();
        foreach (IConfigurationSection entry in section.GetSection("Secrets").GetChildren())
        {
            secrets[entry.Key] = entry.Value ?? string.Empty;
        }

        RuntimeLimits runtimeLimits = section.GetSection("RuntimeLimits").Exists()
            ? new RuntimeLimits(
                section.GetValue("RuntimeLimits:MaxRequestBodySizeBytes", RuntimeLimits.Default.MaxRequestBodySizeBytes),
                TimeSpan.FromSeconds(section.GetValue("RuntimeLimits:RequestTimeoutSeconds", RuntimeLimits.Default.RequestTimeout.TotalSeconds)),
                section.GetValue("RuntimeLimits:MaxConcurrentRequests", RuntimeLimits.Default.MaxConcurrentRequests))
            : RuntimeLimits.Default;

        return new EnvironmentManifest(
            EnvironmentName: section.GetValue<string?>("Name") ?? string.Empty,
            FeatureFlags: featureFlags,
            ConnectionStrings: connectionStrings,
            Secrets: secrets,
            RuntimeLimits: runtimeLimits);
    }
}
