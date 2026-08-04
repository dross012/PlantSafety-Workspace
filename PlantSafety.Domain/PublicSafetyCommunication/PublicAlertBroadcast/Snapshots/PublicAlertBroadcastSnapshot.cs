// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicAlertBroadcastSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PublicAlertBroadcast aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PublicAlertBroadcast"/> aggregate for fast rehydration.</summary>
public sealed record PublicAlertBroadcastSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PublicAlertBroadcastId PublicAlertBroadcastId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PublicAlertBroadcastStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
