// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Domain
//  File:           ChecklistsArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Checklists aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Checklists.Policies;

/// <summary>Determines whether a <see cref="Checklists"/> aggregate is eligible for archival.</summary>
public static class ChecklistsArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Checklists may be archived.</summary>
    public static bool IsEligibleForArchival(Checklists aggregate)
        => aggregate.Status == ChecklistsStatus.Closed;
}
