// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for CommunityConcern aggregate rehydration. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Aggregates.CommunityConcern"/> aggregate for fast rehydration.</summary>
public sealed record CommunityConcernSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public CommunityConcernId CommunityConcernId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public CommunityConcernStatus Status { get; init; }

    /// <summary>Concern category at snapshot time.</summary>
    public CommunityConcernCategory Category { get; init; }

    /// <summary>Free-text severity at snapshot time.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Facility area the concern relates to.</summary>
    public string FacilityArea { get; init; } = string.Empty;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
