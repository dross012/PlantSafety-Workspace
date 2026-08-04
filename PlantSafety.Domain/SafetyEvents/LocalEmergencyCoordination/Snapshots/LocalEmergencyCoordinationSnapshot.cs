// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for LocalEmergencyCoordination aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Aggregates.LocalEmergencyCoordination"/> aggregate for fast rehydration.</summary>
public sealed record LocalEmergencyCoordinationSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public LocalEmergencyCoordinationId LocalEmergencyCoordinationId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public LocalEmergencyCoordinationStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }

    /// <summary>Count of mutual aid agreements established at snapshot time.</summary>
    public int MutualAidAgreementCount { get; init; }

    /// <summary>Count of active joint response plans at snapshot time.</summary>
    public int ActiveJointResponsePlanCount { get; init; }
}
