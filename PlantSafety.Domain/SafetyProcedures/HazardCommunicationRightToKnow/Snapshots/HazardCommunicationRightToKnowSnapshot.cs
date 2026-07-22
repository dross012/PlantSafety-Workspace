// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           HazardCommunicationRightToKnowSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for HazardCommunicationRightToKnow aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Snapshots;

/// <summary>Immutable snapshot of a <see cref="HazardCommunicationRightToKnow"/> aggregate for fast rehydration.</summary>
public sealed record HazardCommunicationRightToKnowSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public HazardCommunicationRightToKnowId HazardCommunicationRightToKnowId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public HazardCommunicationRightToKnowStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}