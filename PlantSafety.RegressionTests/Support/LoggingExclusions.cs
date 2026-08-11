// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RegressionTests.Support
//  Layer:          Tests (Regression)
//  Description:    The complete, reasoned list of mutating procedures that
//                  deliberately do NOT write the canonical audit trail.
//
//                  This list is the point of the coverage tests. Adding an
//                  entry is cheap in mechanics and expensive in intent: it is
//                  a written, reviewable claim that a specific domain write
//                  should go unaudited. A procedure missing its audit trail by
//                  accident fails the test; one missing it on purpose appears
//                  here with the reason. Neither is silent.
// ============================================================================
namespace PlantSafety.RegressionTests.Support;

/// <summary>Procedures intentionally exempt from the audit/error logging requirement.</summary>
public static class LoggingExclusions
{
    /// <summary>Maps an exempt procedure's bare name to the reason it is exempt.</summary>
    public static readonly IReadOnlyDictionary<string, string> AuditExempt =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["usp_RequestMetricEvent_InsertBatch"] =
                "Per-sample platform telemetry. It ingests request metrics in batches at request " +
                "volume, so one audit row per call would grow the audit trail faster than the " +
                "business events it exists to record, and would bury them.",

            ["usp_PermissionUsageLog_Insert"] =
                "This procedure IS the authorization-decision log. Auditing a write to the audit " +
                "surface is circular: the audit row would say nothing the logged row does not " +
                "already say, and would double every authorization check's write cost.",
        };

    /// <summary>Procedures intentionally exempt from writing an ErrorLog row.</summary>
    /// <remarks>Same two, for the same reasons: a failure to record a telemetry sample or a
    /// permission-usage row is not itself a business-significant error, and the caller still sees
    /// the original exception because the rethrow is unchanged.</remarks>
    public static readonly IReadOnlyDictionary<string, string> ErrorExempt = AuditExempt;
}
