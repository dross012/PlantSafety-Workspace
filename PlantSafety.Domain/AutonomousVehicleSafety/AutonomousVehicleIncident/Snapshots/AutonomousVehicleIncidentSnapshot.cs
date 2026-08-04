// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for AutonomousVehicleIncident aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Snapshots;

/// <summary>Immutable snapshot of an <see cref="Aggregates.AutonomousVehicleIncident"/> aggregate for fast rehydration.</summary>
public sealed record AutonomousVehicleIncidentSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AutonomousVehicleIncidentId AutonomousVehicleIncidentId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public AutonomousVehicleIncidentStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
