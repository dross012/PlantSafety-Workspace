// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Authorization policy names for the Compliance module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Compliance.Policies;

/// <summary>Authorization policy name constants for Compliance features.</summary>
public static class ComplianceAuthorizationPolicies
{
    /// <summary>Policy that grants access to create permit-to-work records.</summary>
    public const string CanCreatePermit = "Compliance.CanCreatePermit";

    /// <summary>Policy that grants access to approve permits.  Restricted to Safety Officers and above.</summary>
    public const string CanApprovePermit = "Compliance.CanApprovePermit";

    /// <summary>Policy that grants access to suspend active permits.</summary>
    public const string CanSuspendPermit = "Compliance.CanSuspendPermit";

    /// <summary>Policy that grants access to create MOC requests.</summary>
    public const string CanCreateMoc = "Compliance.CanCreateMoc";

    /// <summary>Policy that grants access to approve MOC requests.</summary>
    public const string CanApproveMoc = "Compliance.CanApproveMoc";

    /// <summary>Policy that grants access to create obligations.</summary>
    public const string CanCreateObligation = "Compliance.CanCreateObligation";

    /// <summary>Policy that grants access to view audit findings.</summary>
    public const string CanViewAuditFindings = "Compliance.CanViewAuditFindings";
}
