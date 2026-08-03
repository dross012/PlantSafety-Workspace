// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           DimAsset.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Asset/equipment dimension using the Slowly Changing Dimension Type 2 pattern.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.UnifiedModel.Abstractions;

namespace PlantSafety.Domain.Analytics.UnifiedModel.Dimensions;

/// <summary>
/// SCD Type 2 dimension row describing a piece of equipment/asset as it was configured
/// (criticality, site assignment, maintenance strategy) during the row's effective window.
/// </summary>
public sealed record DimAsset : ISlowlyChangingDimension
{
    /// <inheritdoc/>
    public required Guid SurrogateKey { get; init; }

    /// <summary>Stable asset/equipment identifier (natural key), same across all row versions.</summary>
    public required Guid BusinessKey { get; init; }

    /// <inheritdoc/>
    public required UtcTimestamp EffectiveFromUtc { get; init; }

    /// <inheritdoc/>
    public UtcTimestamp? EffectiveToUtc { get; init; }

    /// <inheritdoc/>
    public required bool IsCurrent { get; init; }

    /// <inheritdoc/>
    public required int RevisionNumber { get; init; }

    /// <summary>Tenant that owns this asset record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Asset display name/tag as of this row version.</summary>
    public required string AssetName { get; init; }

    /// <summary>Asset class/category as of this row version (e.g. "RotatingEquipment", "PressureVessel").</summary>
    public required string AssetClass { get; init; }

    /// <summary>Surrogate key of the site this asset was installed at as of this row version.</summary>
    public required Guid SiteKey { get; init; }

    /// <summary>Criticality tier as of this row version (e.g. "Critical", "Essential", "NonCritical").</summary>
    public required string CriticalityTier { get; init; }

    /// <summary>Maintenance strategy applied as of this row version ("Preventive", "Predictive", "ReactiveOnly").</summary>
    public required string MaintenanceStrategy { get; init; }
}
