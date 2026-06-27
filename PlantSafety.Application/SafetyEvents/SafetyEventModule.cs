// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: SafetyEvents module registration marker and constants.
//   Covers: Incident Management, Near Miss, Observation, RCA, Crisis Management, Emergency Management, Drills & Exercises.
//   Critical module — highest business continuity priority.
//   All commands, queries, and handlers for this module are in Features/SafetyEvents.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.SafetyEvents;

/// <summary>
/// Module marker and constants for the SafetyEvents bounded context.
/// Covers incident reporting, near-miss, observation, RCA, crisis and emergency management.
/// This is a Critical module with highest business continuity priority.
/// </summary>
public static class SafetyEventModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "SafetyEvents";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "SafetyEvents";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "SafetyEvents";

    /// <summary>Default SLA for incident dispatch: 30 seconds.</summary>
    public const int IncidentDispatchSlaSeconds = 30;

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create an incident report.</summary>
        public const string CreateIncident = "SafetyEvents.Incidents.Create";

        /// <summary>Permission to update an incident report.</summary>
        public const string UpdateIncident = "SafetyEvents.Incidents.Update";

        /// <summary>Permission to close an incident.</summary>
        public const string CloseIncident = "SafetyEvents.Incidents.Close";

        /// <summary>Permission to view incidents.</summary>
        public const string ViewIncident = "SafetyEvents.Incidents.View";

        /// <summary>Permission to create corrective actions.</summary>
        public const string CreateCorrectiveAction = "SafetyEvents.CorrectiveActions.Create";
    }
}
