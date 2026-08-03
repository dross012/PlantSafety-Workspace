// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           DimSite.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Site dimension using the Slowly Changing Dimension Type 2 pattern.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.UnifiedModel.Abstractions;
using PlantSafety.Domain.Analytics.UnifiedModel.ValueObjects.Ids;

namespace PlantSafety.Domain.Analytics.UnifiedModel.Dimensions;

/// <summary>
/// SCD Type 2 dimension row describing a physical site as it was configured during
/// <see cref="ISlowlyChangingDimension.EffectiveFromUtc"/>..<see cref="ISlowlyChangingDimension.EffectiveToUtc"/>.
/// </summary>
public sealed record DimSite : ISlowlyChangingDimension
{
    /// <inheritdoc/>
    public required Guid SurrogateKey { get; init; }

    /// <summary>Stable Site identifier (natural key), same across all row versions.</summary>
    public required Guid BusinessKey { get; init; }

    /// <inheritdoc/>
    public required UtcTimestamp EffectiveFromUtc { get; init; }

    /// <inheritdoc/>
    public UtcTimestamp? EffectiveToUtc { get; init; }

    /// <inheritdoc/>
    public required bool IsCurrent { get; init; }

    /// <inheritdoc/>
    public required int RevisionNumber { get; init; }

    /// <summary>Tenant that owns this site.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site display name as of this row version.</summary>
    public required string SiteName { get; init; }

    /// <summary>Region/geography grouping as of this row version.</summary>
    public required string Region { get; init; }

    /// <summary>Facility type code as of this row version (e.g. "Manufacturing", "Warehouse", "Office").</summary>
    public required string FacilityType { get; init; }

    /// <summary>Operational status as of this row version (e.g. "Active", "Decommissioned").</summary>
    public required string OperationalStatus { get; init; }

    /// <summary>Unified dimension surrogate key wrapper.</summary>
    public UnifiedDimensionId DimensionId => UnifiedDimensionId.FromGuid(SurrogateKey);
}
