// ============================================================================
//  Project:        Shared.Configuration
//  Module:         Abstractions.Validation
//  File:           EnvironmentManifestValidator.cs
//  Created:        2026-08-09
//  Description:    Phase 2.5 deliverable: the startup configuration
//                  validator. Collects every problem (missing keys,
//                  invalid values, out-of-range limits) instead of
//                  throwing on the first one, so a misconfigured
//                  environment is fixed in one pass rather than
//                  discovered error-by-error across repeated restarts.
// ============================================================================
using Shared.Configuration.Abstractions.Manifest;

namespace Shared.Configuration.Abstractions.Validation;

/// <summary>Validates an EnvironmentManifest, collecting every problem found.</summary>
public static class EnvironmentManifestValidator
{
    /// <summary>Validates the given manifest, returning every problem found (empty if valid).</summary>
    public static IReadOnlyList<string> Validate(EnvironmentManifest manifest)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(manifest.EnvironmentName))
        {
            errors.Add("Environment:Name is missing or empty.");
        }

        if (manifest.RuntimeLimits.MaxRequestBodySizeBytes <= 0)
        {
            errors.Add($"Environment:RuntimeLimits:MaxRequestBodySizeBytes must be positive; was {manifest.RuntimeLimits.MaxRequestBodySizeBytes}.");
        }

        if (manifest.RuntimeLimits.RequestTimeout <= TimeSpan.Zero)
        {
            errors.Add($"Environment:RuntimeLimits:RequestTimeoutSeconds must be positive; was {manifest.RuntimeLimits.RequestTimeout.TotalSeconds}.");
        }

        if (manifest.RuntimeLimits.RequestTimeout > TimeSpan.FromMinutes(10))
        {
            errors.Add($"Environment:RuntimeLimits:RequestTimeoutSeconds is implausibly large ({manifest.RuntimeLimits.RequestTimeout.TotalSeconds}s > 600s); check for a misconfigured unit.");
        }

        if (manifest.RuntimeLimits.MaxConcurrentRequests <= 0)
        {
            errors.Add($"Environment:RuntimeLimits:MaxConcurrentRequests must be positive; was {manifest.RuntimeLimits.MaxConcurrentRequests}.");
        }

        foreach ((string key, string value) in manifest.ConnectionStrings)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"Environment:ConnectionStrings:{key} is present but empty.");
            }
        }

        foreach ((string key, string value) in manifest.Secrets)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"Environment:Secrets:{key} is present but empty.");
            }
        }

        return errors;
    }
}
