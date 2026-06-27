// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: ContinuousImprovement module registration marker and constants.
//   Covers: Human Factors, Fatigue Management, Ergonomics, Industrial Hygiene, Worker Well-Being,
//           Equipment Safety, Hazard Assessment, HazMat, Inspections, JSA, LOTO, Permits to Work.
//   All commands, queries, and handlers for this module are in Features/ContinuousImprovement.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.ContinuousImprovement;

/// <summary>
/// Module marker and constants for the ContinuousImprovement bounded context.
/// Covers operational safety improvements spanning human factors, ergonomics, hazard control, and LOTO.
/// </summary>
public static class ContinuousImprovementModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "ContinuousImprovement";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "ContinuousImprovement";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "ContinuousImprovement";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create a hazard assessment.</summary>
        public const string CreateHazardAssessment = "ContinuousImprovement.HazardAssessments.Create";

        /// <summary>Permission to initiate a LOTO procedure.</summary>
        public const string InitiateLOTO = "ContinuousImprovement.LOTO.Initiate";

        /// <summary>Permission to record an inspection.</summary>
        public const string RecordInspection = "ContinuousImprovement.Inspections.Record";

        /// <summary>Permission to perform a JSA.</summary>
        public const string PerformJSA = "ContinuousImprovement.JSA.Perform";

        /// <summary>Permission to manage HazMat records.</summary>
        public const string ManageHazMat = "ContinuousImprovement.HazMat.Manage";
    }
}
