// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Common.Rollout
//  Layer:          Application
//  File:           FeatureRolloutEvaluator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default ITenantRolloutPolicy implementation. Computes a stable hash of
//                  TenantId + FeatureName (SHA-256, not string.GetHashCode() -- .NET's
//                  GetHashCode is explicitly documented as NOT stable across processes/
//                  runs/framework versions, which would make canary membership flip on every
//                  app restart) and takes it modulo 100 to get a deterministic bucket 0-99
//                  for that (tenant, feature) pair. A tenant is in the rollout wave iff its
//                  bucket is less than the configured RolloutPercentage -- so raising the
//                  percentage over time only ever ADDS tenants to the wave, never removes
//                  previously-included ones (a monotonic canary rollout, not a randomly
//                  reshuffled one). Pure, side-effect-free, and fully unit-testable without
//                  any database/network dependency. Additive-only evolution.
// ============================================================================
using System.Security.Cryptography;
using System.Text;

namespace PlantSafety.Application.Common.Rollout;

/// <summary>
/// Deterministic <see cref="ITenantRolloutPolicy"/> implementation based on a stable SHA-256
/// hash of TenantId + FeatureName, modulo 100.
/// </summary>
public sealed class FeatureRolloutEvaluator : ITenantRolloutPolicy
{
    /// <inheritdoc/>
    public bool IsInRollout(Guid tenantId, string featureName, int rolloutPercentage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureName);

        if (rolloutPercentage <= 0)
        {
            return false;
        }

        if (rolloutPercentage >= 100)
        {
            return true;
        }

        var bucket = ComputeBucket(tenantId, featureName);
        return bucket < rolloutPercentage;
    }

    /// <summary>
    /// Computes a stable, deterministic bucket (0-99) for the given tenant/feature pair using
    /// SHA-256 over their concatenated, invariant-culture string forms.
    /// </summary>
    private static int ComputeBucket(Guid tenantId, string featureName)
    {
        var input = $"{tenantId:D}:{featureName}";
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);

        // Use the first 4 bytes of the hash as an unsigned 32-bit integer for the modulo --
        // stable across platforms/endianness since we always read the same fixed byte order.
        var value = ((uint)hash[0] << 24) | ((uint)hash[1] << 16) | ((uint)hash[2] << 8) | hash[3];
        return (int)(value % 100);
    }
}
