// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Operational module registration marker and constants.
//   Covers: Alerts, Auditing, Worker Mgmt, Machine Mgmt, Process Mgmt, Training, Certifications,
//           PPE, Safety Culture, Scheduling, Maintenance, Predictive Maintenance,
//           Production Tracking, Routing, Reporting, Safety Programs, Incident Mgmt, Document Mgmt.
//   All commands, queries, and handlers for this module are in Features/Operational.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.Operational;

/// <summary>
/// Module marker and constants for the Operational bounded context.
/// Covers the full suite of core operational safety management features including
/// worker management, training, PPE, scheduling, maintenance, and reporting.
/// </summary>
public static class OperationalModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "Operational";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "Operational";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "Operational";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to manage worker records.</summary>
        public const string ManageWorkers = "Operational.Workers.Manage";

        /// <summary>Permission to assign training.</summary>
        public const string AssignTraining = "Operational.Training.Assign";

        /// <summary>Permission to issue PPE.</summary>
        public const string IssuePPE = "Operational.PPE.Issue";

        /// <summary>Permission to manage schedules.</summary>
        public const string ManageSchedules = "Operational.Schedules.Manage";

        /// <summary>Permission to view operational reports.</summary>
        public const string ViewReports = "Operational.Reports.View";

        /// <summary>Permission to manage alert routing rules.</summary>
        public const string ManageAlerts = "Operational.Alerts.Manage";
    }
}
