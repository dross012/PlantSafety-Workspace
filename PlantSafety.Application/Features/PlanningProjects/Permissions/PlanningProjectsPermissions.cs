// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Permission constants for the PlanningProjects module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.PlanningProjects.Permissions;

/// <summary>Permission strings for the PlanningProjects module.  Format: <c>Module.Feature.Action</c>.</summary>
public static class PlanningProjectsPermissions
{
    /// <summary>Safety project permissions.</summary>
    public static class Projects
    {
        /// <summary>Create a safety project.</summary>
        public const string Create = "PlanningProjects.Projects.Create";
        /// <summary>Update a safety project.</summary>
        public const string Update = "PlanningProjects.Projects.Update";
        /// <summary>Complete a safety project.</summary>
        public const string Complete = "PlanningProjects.Projects.Complete";
        /// <summary>View a safety project.</summary>
        public const string View = "PlanningProjects.Projects.View";
        /// <summary>List safety projects.</summary>
        public const string List = "PlanningProjects.Projects.List";
    }

    /// <summary>Milestone permissions.</summary>
    public static class Milestones
    {
        /// <summary>Add a project milestone.</summary>
        public const string Add = "PlanningProjects.Milestones.Add";
        /// <summary>Complete a project milestone.</summary>
        public const string Complete = "PlanningProjects.Milestones.Complete";
        /// <summary>View milestones.</summary>
        public const string View = "PlanningProjects.Milestones.View";
    }
}
