// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Domain
//  File:           CultureSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Culture aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Culture.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Culture"/> aggregate for fast rehydration.</summary>
public sealed record CultureSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public CultureId CultureId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public CultureStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
