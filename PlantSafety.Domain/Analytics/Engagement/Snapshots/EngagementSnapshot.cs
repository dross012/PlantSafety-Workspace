// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           EngagementSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Engagement aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Engagement"/> aggregate for fast rehydration.</summary>
public sealed record EngagementSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public EngagementId EngagementId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public EngagementStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
