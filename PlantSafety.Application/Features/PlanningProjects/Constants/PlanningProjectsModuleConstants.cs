// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Module-scoped constants for the PlanningProjects module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.PlanningProjects.Constants;

/// <summary>Module-scoped constants for PlanningProjects.</summary>
public static class PlanningProjectsModuleConstants
{
    /// <summary>Canonical module name.</summary>
    public const string ModuleName = "PlanningProjects";

    /// <summary>SQL schema name.</summary>
    public const string SqlSchema = "PlanningProjects";

    /// <summary>Service Bus topic name.</summary>
    public const string ServiceBusTopic = "planning-projects";

    /// <summary>Project creation SLA in seconds (§ProjectCreationSlaSeconds).</summary>
    public const int ProjectCreationSlaSeconds = 2;

    /// <summary>Milestone update SLA in seconds.</summary>
    public const int MilestoneUpdateSlaSeconds = 1;

    /// <summary>Cache key prefix for project records.</summary>
    public const string ProjectCachePrefix = "projects";

    /// <summary>Project lifecycle statuses in order.</summary>
    public static readonly IReadOnlyList<string> ProjectStatuses =
        ["Draft", "Planned", "InProgress", "Blocked", "Completed", "Archived"];
}
