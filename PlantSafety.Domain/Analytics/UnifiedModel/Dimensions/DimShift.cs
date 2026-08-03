// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           DimShift.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Shift dimension. Low-cardinality reference lookup, not slowly-changing.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.UnifiedModel.Dimensions;

/// <summary>
/// Reference lookup dimension row for a work shift definition (e.g. Day/Swing/Night). Low
/// cardinality and effectively static per site, so it intentionally does not implement
/// <see cref="Abstractions.ISlowlyChangingDimension"/>.
/// </summary>
public sealed record DimShift
{
    /// <summary>Surrogate key of this shift row.</summary>
    public required Guid ShiftKey { get; init; }

    /// <summary>Tenant that owns this shift definition.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Surrogate key of the site this shift is defined for.</summary>
    public required Guid SiteKey { get; init; }

    /// <summary>Shift display name (e.g. "Day", "Swing", "Night").</summary>
    public required string ShiftName { get; init; }

    /// <summary>Local start time of the shift.</summary>
    public required TimeOnly StartTimeLocal { get; init; }

    /// <summary>Local end time of the shift.</summary>
    public required TimeOnly EndTimeLocal { get; init; }

    /// <summary>Shift duration in hours.</summary>
    public required double DurationHours { get; init; }

    /// <summary>True when this shift crosses midnight.</summary>
    public bool CrossesMidnight { get; init; }
}
