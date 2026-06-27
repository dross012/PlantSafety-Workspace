// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Compliance module registration marker and constants.
//   Covers: Compliance, MOC (Management of Change), Policies & Procedures, Regulatory Calendar,
//           Certification, Auditing, and Permit-to-Work.
//   All commands, queries, and handlers for this module are in Features/Compliance.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.Compliance;

/// <summary>
/// Module marker and constants for the Compliance bounded context.
/// Covers regulatory governance, rule evaluation, non-conformance workflows, MOC, permits, and certification.
/// </summary>
public static class ComplianceModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "Compliance";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "Compliance";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "Compliance";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create a compliance obligation.</summary>
        public const string CreateObligation = "Compliance.Obligations.Create";

        /// <summary>Permission to view compliance obligations.</summary>
        public const string ViewObligation = "Compliance.Obligations.View";

        /// <summary>Permission to create a permit.</summary>
        public const string CreatePermit = "Compliance.Permits.Create";

        /// <summary>Permission to approve a permit.</summary>
        public const string ApprovePermit = "Compliance.Permits.Approve";

        /// <summary>Permission to close a permit.</summary>
        public const string ClosePermit = "Compliance.Permits.Close";

        /// <summary>Permission to create a MOC workflow.</summary>
        public const string CreateMOC = "Compliance.MOC.Create";

        /// <summary>Permission to view audit findings.</summary>
        public const string ViewAuditFinding = "Compliance.Auditing.ViewFindings";
    }
}
