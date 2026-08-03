// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Enterprise.MultiRegion
//  Layer:          Application
//  File:           TenantRegionAffinityResolver.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 §4.3 (Global Routing) default ITenantRegionRouter implementation.
//                  Resolution order:
//                    1. An explicit override configured in TenantRegionAffinityOptions
//                       (e.g. an EU tenant pinned to Region.Eu for data-residency reasons).
//                    2. A deterministic, stable SHA-256 hash of the TenantId modulo the number
//                       of known regions -- the same approach already established by
//                       FeatureRolloutEvaluator (Phase 6 §6.2) for the identical reason: .NET's
//                       string.GetHashCode()/object.GetHashCode() are explicitly documented as
//                       NOT stable across processes/runs/framework versions, which would make a
//                       tenant's region assignment flip on every app restart -- unacceptable for
//                       a routing decision that determines which regional database a tenant's
//                       data lives in.
//                  Pure, side-effect-free, fully unit-testable without any database/network
//                  dependency. Additive-only evolution.
// ============================================================================
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Application.Enterprise.MultiRegion;

/// <summary>
/// Default <see cref="ITenantRegionRouter"/> implementation: explicit configured overrides first,
/// falling back to a deterministic SHA-256 hash-based assignment across the known regions.
/// </summary>
public sealed class TenantRegionAffinityResolver : ITenantRegionRouter
{
    private readonly TenantRegionAffinityOptions _options;

    /// <summary>Initialises a new instance of <see cref="TenantRegionAffinityResolver"/>.</summary>
    public TenantRegionAffinityResolver(IOptions<TenantRegionAffinityOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc/>
    public Region ResolveRegion(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId must not be empty.", nameof(tenantId));
        }

        if (_options.Overrides.TryGetValue(tenantId.ToString("D"), out var overrideCode)
            && Region.TryParse(overrideCode, out var overrideRegion))
        {
            return overrideRegion!;
        }

        var bucket = ComputeBucket(tenantId, Region.All.Count);
        return Region.All[bucket];
    }

    /// <summary>
    /// Computes a stable, deterministic bucket (0 to <paramref name="regionCount"/> - 1) for the
    /// given tenant using SHA-256 over its invariant-culture string form.
    /// </summary>
    private static int ComputeBucket(Guid tenantId, int regionCount)
    {
        var bytes = Encoding.UTF8.GetBytes(tenantId.ToString("D"));
        var hash = SHA256.HashData(bytes);

        // Use the first 4 bytes of the hash as an unsigned 32-bit integer for the modulo --
        // stable across platforms/endianness since we always read the same fixed byte order.
        var value = ((uint)hash[0] << 24) | ((uint)hash[1] << 16) | ((uint)hash[2] << 8) | hash[3];
        return (int)(value % (uint)regionCount);
    }
}
