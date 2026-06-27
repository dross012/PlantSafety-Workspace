// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: PlanningProjects module registration marker and constants.
//   Covers: Strategic Planning, Project Governance, Execution Tracking, Project Budgeting, Project Scheduling.
//   All commands, queries, and handlers for this module are in Features/PlanningProjects.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.PlanningProjects;

/// <summary>
/// Module marker and constants for the PlanningProjects bounded context.
/// Covers project definition, milestone planning, resource allocation, risk and compliance alignment.
/// Project lifecycle: Draft → Planned → In Progress → Blocked → Completed → Archived.
/// </summary>
public static class PlanningProjectsModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "PlanningProjects";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "PlanningProjects";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "PlanningProjects";

    /// <summary>SLA for project creation: 2 seconds.</summary>
    public const int ProjectCreationSlaSeconds = 2;

    /// <summary>SLA for milestone update: 1 second.</summary>
    public const int MilestoneUpdateSlaSeconds = 1;

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create a safety project.</summary>
        public const string CreateProject = "PlanningProjects.Projects.Create";

        /// <summary>Permission to update a safety project.</summary>
        public const string UpdateProject = "PlanningProjects.Projects.Update";

        /// <summary>Permission to complete a project.</summary>
        public const string CompleteProject = "PlanningProjects.Projects.Complete";

        /// <summary>Permission to add a milestone to a project.</summary>
        public const string AddMilestone = "PlanningProjects.Milestones.Add";

        /// <summary>Permission to view projects.</summary>
        public const string ViewProject = "PlanningProjects.Projects.View";
    }
}
