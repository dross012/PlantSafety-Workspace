// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Permission constants for the SafetyEvents module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Permissions;

/// <summary>Permission strings for the SafetyEvents module.  Format: <c>Module.Feature.Action</c>.</summary>
public static class SafetyEventsPermissions
{
    /// <summary>Incident management permissions.</summary>
    public static class Incidents
    {
        /// <summary>Create a new incident report.</summary>
        public const string Create = "SafetyEvents.Incidents.Create";
        /// <summary>Update an existing incident.</summary>
        public const string Update = "SafetyEvents.Incidents.Update";
        /// <summary>Soft-delete an incident record.</summary>
        public const string Delete = "SafetyEvents.Incidents.Delete";
        /// <summary>View a single incident.</summary>
        public const string View = "SafetyEvents.Incidents.View";
        /// <summary>List and search incidents.</summary>
        public const string List = "SafetyEvents.Incidents.List";
        /// <summary>Export incident records.</summary>
        public const string Export = "SafetyEvents.Incidents.Export";
    }

    /// <summary>Near miss permissions.</summary>
    public static class NearMisses
    {
        /// <summary>Create a near miss report.</summary>
        public const string Create = "SafetyEvents.NearMisses.Create";
        /// <summary>Update a near miss record.</summary>
        public const string Update = "SafetyEvents.NearMisses.Update";
        /// <summary>View a near miss record.</summary>
        public const string View = "SafetyEvents.NearMisses.View";
        /// <summary>List near miss records.</summary>
        public const string List = "SafetyEvents.NearMisses.List";
    }

    /// <summary>Observation permissions.</summary>
    public static class Observations
    {
        /// <summary>Create an observation record.</summary>
        public const string Create = "SafetyEvents.Observations.Create";
        /// <summary>Update an observation record.</summary>
        public const string Update = "SafetyEvents.Observations.Update";
        /// <summary>View an observation record.</summary>
        public const string View = "SafetyEvents.Observations.View";
        /// <summary>List observation records.</summary>
        public const string List = "SafetyEvents.Observations.List";
    }

    /// <summary>Root-cause analysis permissions.</summary>
    public static class RootCauseAnalysis
    {
        /// <summary>Create an RCA.</summary>
        public const string Create = "SafetyEvents.RootCauseAnalysis.Create";
        /// <summary>View an RCA.</summary>
        public const string View = "SafetyEvents.RootCauseAnalysis.View";
    }

    /// <summary>Crisis management permissions.</summary>
    public static class CrisisManagement
    {
        /// <summary>Declare a crisis event.  Restricted to Safety Managers and above.</summary>
        public const string Declare = "SafetyEvents.CrisisManagement.Declare";
        /// <summary>View a crisis event.</summary>
        public const string View = "SafetyEvents.CrisisManagement.View";
    }

    /// <summary>Emergency response permissions.</summary>
    public static class EmergencyManagement
    {
        /// <summary>Activate an emergency response.</summary>
        public const string Activate = "SafetyEvents.EmergencyManagement.Activate";
        /// <summary>View an emergency response.</summary>
        public const string View = "SafetyEvents.EmergencyManagement.View";
    }

    /// <summary>Drills and exercises permissions.</summary>
    public static class Drills
    {
        /// <summary>Schedule a drill.</summary>
        public const string Schedule = "SafetyEvents.Drills.Schedule";
        /// <summary>Record drill results.</summary>
        public const string RecordResults = "SafetyEvents.Drills.RecordResults";
        /// <summary>View drill records.</summary>
        public const string View = "SafetyEvents.Drills.View";
    }
}
