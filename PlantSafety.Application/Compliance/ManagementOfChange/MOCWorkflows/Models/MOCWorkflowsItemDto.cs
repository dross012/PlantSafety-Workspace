// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Management of Change workflow record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.ManagementOfChange.MOCWorkflows.Models;

/// <summary>A single Management of Change workflow instance.</summary>
public sealed record MOCWorkflowsItemDto
{
    /// <summary>Workflow identifier.</summary>
    public required Guid WorkflowId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Short title describing the proposed change.</summary>
    public required string ChangeTitle { get; init; }

    /// <summary>Current workflow stage (e.g., Initiation, RiskReview, Approval, Implementation).</summary>
    public required string CurrentStage { get; init; }

    /// <summary>Current workflow status (e.g., Open/OnHold/Completed/Cancelled).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the workflow was initiated.</summary>
    public required DateTime InitiatedAtUtc { get; init; }

    /// <summary>Identity of the user who initiated the workflow.</summary>
    public required Guid InitiatedBy { get; init; }
}
