// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for AutomatedIncidentDetection aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Snapshots;

/// <summary>Immutable snapshot of a <see cref="AutomatedIncidentDetection"/> aggregate for fast rehydration.</summary>
public sealed record AutomatedIncidentDetectionSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AutomatedIncidentDetectionId AutomatedIncidentDetectionId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public AutomatedIncidentDetectionStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}