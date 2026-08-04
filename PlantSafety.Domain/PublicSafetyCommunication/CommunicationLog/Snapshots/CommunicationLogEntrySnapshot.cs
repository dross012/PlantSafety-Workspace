// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationLogEntrySnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for CommunicationLogEntry aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Snapshots;

/// <summary>Immutable snapshot of a <see cref="CommunicationLogEntry"/> aggregate for fast rehydration.</summary>
public sealed record CommunicationLogEntrySnapshot
{
    /// <summary>Aggregate identity.</summary>
    public CommunicationLogEntryId CommunicationLogEntryId { get; init; }

    /// <summary>Direction at snapshot time.</summary>
    public CommunicationDirection Direction { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
