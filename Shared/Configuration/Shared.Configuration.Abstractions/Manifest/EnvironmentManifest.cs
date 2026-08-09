// ============================================================================
//  Project:        Shared.Configuration
//  Module:         Abstractions.Manifest
//  File:           EnvironmentManifest.cs
//  Created:        2026-08-09
//  Description:    Phase 2.5 deliverable: the environment manifest schema.
//                  One place a host's environment-dependent settings live,
//                  loaded once at startup (EnvironmentManifestLoader) and
//                  validated before the host is allowed to start
//                  (EnvironmentManifestValidator). Secrets are never
//                  logged or included in any diagnostic output -- see
//                  EnvironmentManifestLoader's redaction note.
// ============================================================================
namespace Shared.Configuration.Abstractions.Manifest;

/// <summary>A host's environment-dependent configuration, loaded and validated once at startup.</summary>
public sealed record EnvironmentManifest(
    string EnvironmentName,
    IReadOnlyDictionary<string, bool> FeatureFlags,
    IReadOnlyDictionary<string, string> ConnectionStrings,
    IReadOnlyDictionary<string, string> Secrets,
    RuntimeLimits RuntimeLimits)
{
    /// <summary>Returns the given feature flag's value, or false if not configured.</summary>
    public bool IsFeatureEnabled(string flagName) => FeatureFlags.TryGetValue(flagName, out bool value) && value;
}
