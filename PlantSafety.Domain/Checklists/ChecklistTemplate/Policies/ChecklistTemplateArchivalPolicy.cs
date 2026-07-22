// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for ChecklistTemplate aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Policies;

/// <summary>Determines whether a <see cref="ChecklistTemplate"/> aggregate is eligible for archival.</summary>
public static class ChecklistTemplateArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the ChecklistTemplate may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate aggregate)
        => aggregate.Status == ChecklistTemplateStatus.Closed;
}