// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PermitWorkflow record. Core permit lifecycle workflow: request, hazard assessment, control implementation, authorization, execution, close-out and review.
// SchemaVersion: 1

namespace PlantSafety.Application.PermitToWork.PermitWorkflow.Models;

/// <summary>A single PermitWorkflow read-model record.</summary>
public sealed record PermitWorkflowItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PermitWorkflowId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>PermitNumber.</summary>
    public required  string PermitNumber { get; init; }

    /// <summary>PermitTypeCode.</summary>
    public required  string PermitTypeCode { get; init; }

    /// <summary>RequesterUserId.</summary>
    public Guid? RequesterUserId { get; init; }

    /// <summary>IssuerUserId.</summary>
    public Guid? IssuerUserId { get; init; }

    /// <summary>WorkDescription.</summary>
    public required  string WorkDescription { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}