// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the ChecklistTemplate aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="ChecklistTemplate"/> aggregate.</summary>
public static class ChecklistTemplateLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(ChecklistTemplateStatus current) => current == ChecklistTemplateStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(ChecklistTemplateStatus current) => current == ChecklistTemplateStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(ChecklistTemplateStatus current) => current == ChecklistTemplateStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(ChecklistTemplateStatus current) => current == ChecklistTemplateStatus.Closed;
}