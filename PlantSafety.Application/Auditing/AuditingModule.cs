// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Auditing module registration marker and constants. All commands, queries, and handlers for this module live under PlantSafety.Application.Auditing.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing;

/// <summary>
/// Module marker and constants for the Auditing bounded context.
/// </summary>
public static class AuditingModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "Auditing";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "Auditing";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "Auditing";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create an audit engagement.</summary>
        public const string CreateAuditEngagement = "Auditing.AuditEngagement.Create";

        /// <summary>Permission to view audit engagements.</summary>
        public const string ViewAuditEngagement = "Auditing.AuditEngagement.View";

        /// <summary>Permission to create an audit finding.</summary>
        public const string CreateAuditFinding = "Auditing.AuditFinding.Create";

        /// <summary>Permission to view audit findings.</summary>
        public const string ViewAuditFinding = "Auditing.AuditFinding.View";
    }
}