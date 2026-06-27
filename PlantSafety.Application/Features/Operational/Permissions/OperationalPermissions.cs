// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Permission constants for the Operational module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Operational.Permissions;

/// <summary>Permission strings for the Operational module.  Format: <c>Module.Feature.Action</c>.</summary>
public static class OperationalPermissions
{
    /// <summary>Worker management permissions.</summary>
    public static class Workers
    {
        /// <summary>Manage worker records.</summary>
        public const string Manage = "Operational.Workers.Manage";
        /// <summary>View worker records.</summary>
        public const string View = "Operational.Workers.View";
    }

    /// <summary>Training permissions.</summary>
    public static class Training
    {
        /// <summary>Assign training to a worker.</summary>
        public const string Assign = "Operational.Training.Assign";
        /// <summary>Complete a training record.</summary>
        public const string Complete = "Operational.Training.Complete";
        /// <summary>View training records.</summary>
        public const string View = "Operational.Training.View";
        /// <summary>List training records.</summary>
        public const string List = "Operational.Training.List";
    }

    /// <summary>PPE management permissions.</summary>
    public static class PPEManagement
    {
        /// <summary>Issue PPE to a worker.</summary>
        public const string Issue = "Operational.PPEManagement.Issue";
        /// <summary>View PPE records.</summary>
        public const string View = "Operational.PPEManagement.View";
    }

    /// <summary>Schedule management permissions.</summary>
    public static class Schedules
    {
        /// <summary>Manage worker schedules.</summary>
        public const string Manage = "Operational.Schedules.Manage";
        /// <summary>View schedules.</summary>
        public const string View = "Operational.Schedules.View";
    }

    /// <summary>Hazard assessment permissions.</summary>
    public static class HazardAssessments
    {
        /// <summary>Create a hazard assessment.</summary>
        public const string Create = "Operational.HazardAssessments.Create";
        /// <summary>View a hazard assessment.</summary>
        public const string View = "Operational.HazardAssessments.View";
    }
}
