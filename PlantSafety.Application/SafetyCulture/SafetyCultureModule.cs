// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: SafetyCulture module registration marker and constants. All commands, queries, and handlers for this module live under PlantSafety.Application.SafetyCulture.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyCulture;

/// <summary>
/// Module marker and constants for the SafetyCulture bounded context.
/// </summary>
public static class SafetyCultureModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "SafetyCulture";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "SafetyCulture";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "SafetyCulture";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create a culture assessment.</summary>
        public const string CreateCultureAssessment = "SafetyCulture.CultureAssessment.Create";

        /// <summary>Permission to view culture assessments.</summary>
        public const string ViewCultureAssessment = "SafetyCulture.CultureAssessment.View";

        /// <summary>Permission to record a stop-work event.</summary>
        public const string CreateStopWorkEvent = "SafetyCulture.StopWorkEvent.Create";

        /// <summary>Permission to view stop-work events.</summary>
        public const string ViewStopWorkEvent = "SafetyCulture.StopWorkEvent.View";
    }
}