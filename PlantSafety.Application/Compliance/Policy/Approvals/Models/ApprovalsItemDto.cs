// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Approvals record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Policy.Approvals.Models;

/// <summary>A single Approvals read-model item.</summary>
public sealed record ApprovalsItemDto
{
    /// <summary>Approval Id identifier.</summary>
    public required Guid ApprovalId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Policy Id.</summary>
    public required Guid PolicyId { get; init; }

    /// <summary>Policy Title.</summary>
    public required string PolicyTitle { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Decision At Utc.</summary>
    public required DateTime DecisionAtUtc { get; init; }

    /// <summary>Approver Id.</summary>
    public required Guid ApproverId { get; init; }
}
