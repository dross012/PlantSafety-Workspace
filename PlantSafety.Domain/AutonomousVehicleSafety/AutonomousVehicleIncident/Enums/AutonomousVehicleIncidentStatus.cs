// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the AutonomousVehicleIncident aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.AutonomousVehicleIncident"/> aggregate root.</summary>
public enum AutonomousVehicleIncidentStatus
{
    /// <summary>Incident reported, awaiting investigation.</summary>
    Reported = 0,

    /// <summary>Incident is under active investigation.</summary>
    UnderInvestigation = 1,

    /// <summary>Corrective action is in progress.</summary>
    CorrectiveActionInProgress = 2,

    /// <summary>Incident closed.</summary>
    Closed = 3,
}
