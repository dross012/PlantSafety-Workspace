// ============================================================================
//  Project:        Shared.Configuration
//  Module:         Abstractions.Secrets
//  File:           SecretRedactor.cs
//  Created:        2026-08-09
//  Description:    Phase 3.2 deliverable: the one place an EnvironmentManifest
//                  is turned into something safe to log or expose in
//                  diagnostics. Secrets and connection-string values are never
//                  passed to ILogEventWriter/console/exception output directly
//                  (see EnvironmentManifestLoader's redaction note) -- this
//                  type is what a diagnostics endpoint or startup log line
//                  should use instead of touching manifest.Secrets or
//                  manifest.ConnectionStrings itself.
// ============================================================================
using Shared.Configuration.Abstractions.Manifest;

namespace Shared.Configuration.Abstractions.Secrets;

/// <summary>A safe-to-log view of an EnvironmentManifest: key names and counts, never values.</summary>
public sealed record RedactedEnvironmentManifest(
    string EnvironmentName,
    IReadOnlyList<string> EnabledFeatureFlags,
    IReadOnlyList<string> ConnectionStringKeys,
    IReadOnlyList<string> SecretKeys);

/// <summary>Produces a redacted, log-safe view of an EnvironmentManifest.</summary>
public static class SecretRedactor
{
    /// <summary>Redacts the given manifest: key names are preserved, values are dropped entirely.</summary>
    public static RedactedEnvironmentManifest Redact(EnvironmentManifest manifest)
    {
        return new RedactedEnvironmentManifest(
            EnvironmentName: manifest.EnvironmentName,
            EnabledFeatureFlags: manifest.FeatureFlags.Where(kv => kv.Value).Select(kv => kv.Key).OrderBy(k => k, StringComparer.Ordinal).ToArray(),
            ConnectionStringKeys: manifest.ConnectionStrings.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray(),
            SecretKeys: manifest.Secrets.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray());
    }

    /// <summary>Masks a single secret value for display: first/last character kept, everything else replaced. Empty/short values are fully masked.</summary>
    public static string Mask(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Length <= 4)
        {
            return new string('*', value.Length);
        }

        return $"{value[0]}{new string('*', value.Length - 2)}{value[^1]}";
    }
}
