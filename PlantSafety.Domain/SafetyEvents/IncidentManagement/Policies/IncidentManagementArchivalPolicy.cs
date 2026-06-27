// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for IncidentManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Policies;

/// <summary>Determines whether a <see cref="IncidentManagement"/> aggregate is eligible for archival.</summary>
public static class IncidentManagementArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the IncidentManagement may be archived.</summary>
    public static bool IsEligibleForArchival(IncidentManagement aggregate)
        => aggregate.Status == IncidentManagementStatus.Closed;
}
