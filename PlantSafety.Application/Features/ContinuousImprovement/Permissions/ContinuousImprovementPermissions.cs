// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Permission constants for the ContinuousImprovement module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.ContinuousImprovement.Permissions;

/// <summary>Permission strings for the ContinuousImprovement module.  Format: <c>Module.Feature.Action</c>.</summary>
public static class ContinuousImprovementPermissions
{
    /// <summary>Corrective and preventive action (CAPA) permissions.</summary>
    public static class CAPA
    {
        /// <summary>Create a corrective or preventive action.</summary>
        public const string Create = "ContinuousImprovement.CAPA.Create";
        /// <summary>Update a CAPA record.</summary>
        public const string Update = "ContinuousImprovement.CAPA.Update";
        /// <summary>Close a CAPA record.</summary>
        public const string Close = "ContinuousImprovement.CAPA.Close";
        /// <summary>View a CAPA record.</summary>
        public const string View = "ContinuousImprovement.CAPA.View";
        /// <summary>List CAPA records.</summary>
        public const string List = "ContinuousImprovement.CAPA.List";
    }

    /// <summary>Hazard assessment permissions.</summary>
    public static class HazardAssessments
    {
        /// <summary>Create a hazard assessment.</summary>
        public const string Create = "ContinuousImprovement.HazardAssessments.Create";
        /// <summary>View a hazard assessment.</summary>
        public const string View = "ContinuousImprovement.HazardAssessments.View";
    }

    /// <summary>LOTO (Lockout-Tagout) permissions.</summary>
    public static class LOTO
    {
        /// <summary>Initiate a LOTO procedure.</summary>
        public const string Initiate = "ContinuousImprovement.LOTO.Initiate";
        /// <summary>Release a LOTO procedure.</summary>
        public const string Release = "ContinuousImprovement.LOTO.Release";
    }

    /// <summary>Inspection permissions.</summary>
    public static class Inspections
    {
        /// <summary>Record an inspection.</summary>
        public const string Record = "ContinuousImprovement.Inspections.Record";
        /// <summary>View inspections.</summary>
        public const string View = "ContinuousImprovement.Inspections.View";
    }

    /// <summary>Job Safety Analysis permissions.</summary>
    public static class JSA
    {
        /// <summary>Perform a JSA.</summary>
        public const string Perform = "ContinuousImprovement.JSA.Perform";
        /// <summary>View JSA records.</summary>
        public const string View = "ContinuousImprovement.JSA.View";
    }
}
