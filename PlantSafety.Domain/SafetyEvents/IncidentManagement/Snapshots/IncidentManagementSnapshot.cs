// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for IncidentManagement aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Snapshots;

/// <summary>Immutable snapshot of a <see cref="IncidentManagement"/> aggregate for fast rehydration.</summary>
public sealed record IncidentManagementSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public IncidentManagementId IncidentManagementId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public IncidentManagementStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
