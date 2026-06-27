// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictiveAnalyticsSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PredictiveAnalytics aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PredictiveAnalytics"/> aggregate for fast rehydration.</summary>
public sealed record PredictiveAnalyticsSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PredictiveAnalyticsId PredictiveAnalyticsId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PredictiveAnalyticsStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
