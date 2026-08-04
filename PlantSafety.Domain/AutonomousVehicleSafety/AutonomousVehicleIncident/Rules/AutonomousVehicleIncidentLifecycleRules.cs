// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the AutonomousVehicleIncident aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.AutonomousVehicleIncident"/> aggregate.</summary>
public static class AutonomousVehicleIncidentLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when investigation may begin from <paramref name="current"/>.</summary>
    public static bool CanBeginInvestigation(AutonomousVehicleIncidentStatus current) => current == AutonomousVehicleIncidentStatus.Reported;

    /// <summary>Returns <see langword="true"/> when a suspension review may be triggered from <paramref name="current"/>.</summary>
    public static bool CanTriggerSuspensionReview(AutonomousVehicleIncidentStatus current) =>
        current is AutonomousVehicleIncidentStatus.Reported or AutonomousVehicleIncidentStatus.UnderInvestigation;

    /// <summary>Returns <see langword="true"/> when the incident may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(AutonomousVehicleIncidentStatus current) => current != AutonomousVehicleIncidentStatus.Closed;
}
