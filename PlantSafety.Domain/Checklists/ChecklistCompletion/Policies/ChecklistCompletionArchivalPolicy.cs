// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletionArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for ChecklistCompletion aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Policies;

/// <summary>Determines whether a <see cref="ChecklistCompletion"/> aggregate is eligible for archival.</summary>
public static class ChecklistCompletionArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the ChecklistCompletion may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion aggregate)
        => aggregate.Status == ChecklistCompletionStatus.Closed;
}