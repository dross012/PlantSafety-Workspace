// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Authorization policy names for the PlanningProjects module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.PlanningProjects.Policies;

/// <summary>Authorization policy name constants for PlanningProjects features.</summary>
public static class PlanningProjectsAuthorizationPolicies
{
    /// <summary>Policy that grants access to create safety projects.</summary>
    public const string CanCreateProject = "PlanningProjects.CanCreateProject";

    /// <summary>Policy that grants access to update safety projects.</summary>
    public const string CanUpdateProject = "PlanningProjects.CanUpdateProject";

    /// <summary>Policy that grants access to complete a safety project.</summary>
    public const string CanCompleteProject = "PlanningProjects.CanCompleteProject";

    /// <summary>Policy that grants access to add milestones.</summary>
    public const string CanAddMilestone = "PlanningProjects.CanAddMilestone";

    /// <summary>Policy that grants access to complete milestones.</summary>
    public const string CanCompleteMilestone = "PlanningProjects.CanCompleteMilestone";
}
