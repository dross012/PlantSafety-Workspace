// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs mirroring the three Management of
//   Change read-model endpoints (MOCWorkflows, Approvals, RiskAssessments).
//   Deserialized directly into the inner Data shape of the ApiResponse&lt;T&gt;
//   envelope, matching the established sibling convention (see
//   Modules/PlanningProjects/Models/PlanningProjectsListResponseDto.cs).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.MOC.Models;

/// <summary>A single Management of Change workflow instance (GET v1/compliance/management-of-change/mocworkflows).</summary>
public sealed class MocWorkflowItemDto
{
    /// <summary>Workflow identifier.</summary>
    public Guid WorkflowId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Short title describing the proposed change.</summary>
    public string ChangeTitle { get; init; } = string.Empty;

    /// <summary>Current workflow stage (e.g., Initiation, RiskReview, Approval, Implementation).</summary>
    public string CurrentStage { get; init; } = string.Empty;

    /// <summary>Current workflow status (e.g., Open/OnHold/Completed/Cancelled).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the workflow was initiated.</summary>
    public DateTime InitiatedAtUtc { get; init; }

    /// <summary>Identity of the user who initiated the workflow.</summary>
    public Guid InitiatedBy { get; init; }
}

/// <summary>Response payload for the MOC Workflows read-model list endpoint.</summary>
public sealed class MocWorkflowsResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching workflow items. Never null; empty when no results.</summary>
    public IReadOnlyList<MocWorkflowItemDto> Items { get; init; } = [];
}

/// <summary>A single approval decision recorded against a change request (GET v1/compliance/management-of-change/approvals).</summary>
public sealed class MocApprovalItemDto
{
    /// <summary>Approval record identifier.</summary>
    public Guid ApprovalId { get; init; }

    /// <summary>Identifier of the change request this approval applies to.</summary>
    public Guid ChangeRequestId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the designated approver.</summary>
    public string ApproverName { get; init; } = string.Empty;

    /// <summary>Current approval status (e.g., Pending/Approved/Rejected).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the approval decision was made.</summary>
    public DateTime DecidedAtUtc { get; init; }

    /// <summary>Identity of the user who recorded the approval decision.</summary>
    public Guid ApprovedBy { get; init; }
}

/// <summary>Response payload for the MOC Approvals read-model list endpoint.</summary>
public sealed class MocApprovalsResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching approval items. Never null; empty when no results.</summary>
    public IReadOnlyList<MocApprovalItemDto> Items { get; init; } = [];
}

/// <summary>A single risk assessment performed against a change request (GET v1/compliance/management-of-change/riskassessments).</summary>
public sealed class MocRiskAssessmentItemDto
{
    /// <summary>Risk assessment identifier.</summary>
    public Guid RiskAssessmentId { get; init; }

    /// <summary>Identifier of the change request this assessment applies to.</summary>
    public Guid ChangeRequestId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Assessed risk level (e.g., Low/Medium/High/Critical).</summary>
    public string RiskLevel { get; init; } = string.Empty;

    /// <summary>Current assessment status (e.g., Draft/InReview/Finalized).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the risk assessment was performed.</summary>
    public DateTime AssessedAtUtc { get; init; }

    /// <summary>Identity of the user who performed the risk assessment.</summary>
    public Guid AssessedBy { get; init; }
}

/// <summary>Response payload for the MOC Risk Assessments read-model list endpoint.</summary>
public sealed class MocRiskAssessmentsResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching risk assessment items. Never null; empty when no results.</summary>
    public IReadOnlyList<MocRiskAssessmentItemDto> Items { get; init; } = [];
}
