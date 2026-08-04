// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for LocalEmergencyCoordination aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Policies;

/// <summary>Determines whether a <see cref="Aggregates.LocalEmergencyCoordination"/> aggregate is eligible for archival.</summary>
public static class LocalEmergencyCoordinationArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the LocalEmergencyCoordination case may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination aggregate)
        => aggregate.Status == LocalEmergencyCoordinationStatus.Closed;
}
