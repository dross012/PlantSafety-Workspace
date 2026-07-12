// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Management of Change approval record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.ManagementOfChange.Approvals.Models;

/// <summary>A single approval decision recorded against a change request.</summary>
public sealed record ApprovalsItemDto
{
    /// <summary>Approval record identifier.</summary>
    public required Guid ApprovalId { get; init; }

    /// <summary>Identifier of the change request this approval applies to.</summary>
    public required Guid ChangeRequestId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the designated approver.</summary>
    public required string ApproverName { get; init; }

    /// <summary>Current approval status (e.g., Pending/Approved/Rejected).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the approval decision was made.</summary>
    public required DateTime DecidedAtUtc { get; init; }

    /// <summary>Identity of the user who recorded the approval decision.</summary>
    public required Guid ApprovedBy { get; init; }
}
