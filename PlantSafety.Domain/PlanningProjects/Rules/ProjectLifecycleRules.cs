// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Domain
//  File:           ProjectLifecycleRules.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Guards for the Project aggregate's lifecycle transitions. Grounded in the
//                  pre-existing ProjectStatus enum's documented vocabulary (Planning/Active/
//                  OnHold/Completed/Cancelled/Archived).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PlanningProjects.Rules;

/// <summary>Defines permitted lifecycle transitions for the Project aggregate.</summary>
public static class ProjectLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a milestone may be added while in <paramref name="current"/>.</summary>
    public static bool CanAddMilestone(ProjectStatus current) => current is ProjectStatus.Planning or ProjectStatus.Active;

    /// <summary>Returns <see langword="true"/> when a milestone may be achieved while in <paramref name="current"/>.</summary>
    public static bool CanAchieveMilestone(ProjectStatus current) => current == ProjectStatus.Active;

    /// <summary>Returns <see langword="true"/> when the project may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(ProjectStatus current) => current == ProjectStatus.Planning;

    /// <summary>Returns <see langword="true"/> when the project may be put on hold from <paramref name="current"/>.</summary>
    public static bool CanPutOnHold(ProjectStatus current) => current == ProjectStatus.Active;

    /// <summary>Returns <see langword="true"/> when the project may resume from <paramref name="current"/>.</summary>
    public static bool CanResume(ProjectStatus current) => current == ProjectStatus.OnHold;

    /// <summary>Returns <see langword="true"/> when the project may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(ProjectStatus current) => current == ProjectStatus.Active;

    /// <summary>Returns <see langword="true"/> when the project may be cancelled from <paramref name="current"/>.</summary>
    public static bool CanCancel(ProjectStatus current) => current is ProjectStatus.Planning or ProjectStatus.Active or ProjectStatus.OnHold;

    /// <summary>Returns <see langword="true"/> when the project may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(ProjectStatus current) => current is ProjectStatus.Completed or ProjectStatus.Cancelled;
}
