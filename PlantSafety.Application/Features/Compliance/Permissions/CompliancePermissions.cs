// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Permission constants for the Compliance module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Compliance.Permissions;

/// <summary>Permission strings for the Compliance module.  Format: <c>Module.Feature.Action</c>.</summary>
public static class CompliancePermissions
{
    /// <summary>Regulatory obligation permissions.</summary>
    public static class Obligations
    {
        /// <summary>Create a regulatory obligation.</summary>
        public const string Create = "Compliance.Obligations.Create";
        /// <summary>View a regulatory obligation.</summary>
        public const string View = "Compliance.Obligations.View";
        /// <summary>List regulatory obligations.</summary>
        public const string List = "Compliance.Obligations.List";
    }

    /// <summary>Permit-to-work permissions.</summary>
    public static class Permits
    {
        /// <summary>Create a new permit.</summary>
        public const string Create = "Compliance.Permits.Create";
        /// <summary>Approve a pending permit.</summary>
        public const string Approve = "Compliance.Permits.Approve";
        /// <summary>Suspend an active permit.</summary>
        public const string Suspend = "Compliance.Permits.Suspend";
        /// <summary>Close a permit.</summary>
        public const string Close = "Compliance.Permits.Close";
        /// <summary>View a permit.</summary>
        public const string View = "Compliance.Permits.View";
        /// <summary>List permits.</summary>
        public const string List = "Compliance.Permits.List";
    }

    /// <summary>Management of Change permissions.</summary>
    public static class ManagementOfChange
    {
        /// <summary>Create a MOC request.</summary>
        public const string Create = "Compliance.ManagementOfChange.Create";
        /// <summary>Approve a MOC request.</summary>
        public const string Approve = "Compliance.ManagementOfChange.Approve";
        /// <summary>Reject a MOC request.</summary>
        public const string Reject = "Compliance.ManagementOfChange.Reject";
        /// <summary>View a MOC record.</summary>
        public const string View = "Compliance.ManagementOfChange.View";
    }

    /// <summary>Audit permissions.</summary>
    public static class Audits
    {
        /// <summary>Create an audit record.</summary>
        public const string Create = "Compliance.Audits.Create";
        /// <summary>View audit findings.</summary>
        public const string ViewFindings = "Compliance.Audits.ViewFindings";
        /// <summary>Close an audit finding.</summary>
        public const string CloseFinding = "Compliance.Audits.CloseFinding";
    }
}
