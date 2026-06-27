// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Validation constants specific to the PlanningProjects module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.PlanningProjects.Validation;

/// <summary>Validation constraint constants for PlanningProjects features.</summary>
public static class PlanningProjectsValidationConstants
{
    /// <summary>Maximum project title length.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Maximum project description length.</summary>
    public const int MaxDescriptionLength = 4_000;

    /// <summary>Maximum project type discriminator length.</summary>
    public const int MaxProjectTypeLength = 100;

    /// <summary>Maximum milestone title length.</summary>
    public const int MaxMilestoneTitleLength = 200;

    /// <summary>Maximum number of open milestones per project.</summary>
    public const int MaxOpenMilestonesPerProject = 100;
}
