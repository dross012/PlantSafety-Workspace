// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Domain
//  File:           ProjectMilestoneNotFoundException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when AchieveMilestone cannot find the requested milestone id within a
//                  Project aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PlanningProjects.Exceptions;

/// <summary>Thrown when a Project's milestone lookup by id finds no match.</summary>
public sealed class ProjectMilestoneNotFoundException : ProjectDomainException
{
    /// <summary>Creates a not-found exception for the given Project/milestone identity pair.</summary>
    public ProjectMilestoneNotFoundException(
        ProjectId projectId, Guid milestoneId,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Milestone '{milestoneId}' was not found on Project '{projectId}'.",
            "Project.MilestoneNotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
