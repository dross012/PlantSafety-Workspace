// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           TenantFeatureFlagOverride.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.2 Tenant Configuration)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object representing a single tenant-scoped override of a
//                  platform feature flag. Closes the gap noted in Phase 7 §2.2: the pre-existing
//                  Operations.Configuration feature-flag system (IFeatureFlagRepository /
//                  FeatureFlagDto) is DB-level/ops-controlled (ArchiveDB/IntegrationDB), with no
//                  tenant scoping at all. Rather than retrofitting that ops-facing system (which
//                  would conflate platform-operations flags with tenant self-service
//                  configuration), tenant-scoped overrides are modeled here, as first-class,
//                  tenant-owned data on the Tenant aggregate's TenantConfiguration child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Tenancy.Tenant.ValueObjects;

/// <summary>A single tenant-scoped feature-flag override.</summary>
/// <param name="FlagName">The flag identifier being overridden for this tenant (e.g. <c>"Analytics.PredictiveInsights"</c>).</param>
/// <param name="IsEnabled">The tenant-specific enabled/disabled value for this flag, overriding any platform default.</param>
public sealed record TenantFeatureFlagOverride(string FlagName, bool IsEnabled)
{
    /// <summary>Creates a validated TenantFeatureFlagOverride.</summary>
    public static TenantFeatureFlagOverride Create(string flagName, bool isEnabled)
    {
        if (string.IsNullOrWhiteSpace(flagName))
            throw new ArgumentException("TenantFeatureFlagOverride.FlagName must not be empty.", nameof(flagName));

        return new TenantFeatureFlagOverride(flagName.Trim(), isEnabled);
    }
}
