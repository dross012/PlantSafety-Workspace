// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for AutonomousVehicleIncident aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Policies;

/// <summary>Determines whether an <see cref="Aggregates.AutonomousVehicleIncident"/> aggregate is eligible for archival.</summary>
public static class AutonomousVehicleIncidentArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the incident record may be archived (closed only).</summary>
    public static bool IsEligibleForArchival(Aggregates.AutonomousVehicleIncident aggregate)
        => aggregate.Status == AutonomousVehicleIncidentStatus.Closed;
}
