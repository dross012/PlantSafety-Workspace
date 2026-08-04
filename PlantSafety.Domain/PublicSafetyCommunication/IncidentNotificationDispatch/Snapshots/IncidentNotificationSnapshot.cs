// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IncidentNotificationSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for IncidentNotification aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Snapshots;

/// <summary>Immutable snapshot of an <see cref="IncidentNotification"/> aggregate for fast rehydration.</summary>
public sealed record IncidentNotificationSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public IncidentNotificationId IncidentNotificationId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public IncidentNotificationStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
