// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Domain
//  File:           ScoreCardsSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for ScoreCards aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ScoreCards.Snapshots;

/// <summary>Immutable snapshot of a <see cref="ScoreCards"/> aggregate for fast rehydration.</summary>
public sealed record ScoreCardsSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ScoreCardsId ScoreCardsId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ScoreCardsStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
