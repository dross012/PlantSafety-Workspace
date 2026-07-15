// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Display-ready view models for the Management of Change
//   module — a list-row projection of MocWorkflowItemDto, plus a full
//   approval-workflow-style detail view model that also surfaces the
//   correlated Approvals/RiskAssessments records for the same
//   ChangeRequestId (WorkflowId).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.MOC.Models;

/// <summary>List-row view model for a single MOC workflow.</summary>
public sealed class MocRowViewModel
{
    /// <summary>Gets the workflow identifier.</summary>
    public required Guid WorkflowId { get; init; }

    /// <summary>Gets the change title.</summary>
    public required string ChangeTitle { get; init; }

    /// <summary>Gets the current workflow stage.</summary>
    public required string CurrentStage { get; init; }

    /// <summary>Gets the raw status code.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the UTC timestamp when the workflow was initiated.</summary>
    public required DateTime InitiatedAtUtc { get; init; }
}

/// <summary>A single approval row rendered within the MOC detail page's Approvals tab.</summary>
public sealed class MocApprovalRowViewModel
{
    /// <summary>Gets the approval identifier.</summary>
    public required Guid ApprovalId { get; init; }

    /// <summary>Gets the designated approver's display name.</summary>
    public required string ApproverName { get; init; }

    /// <summary>Gets the raw approval status code.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the UTC timestamp when the decision was recorded.</summary>
    public required DateTime DecidedAtUtc { get; init; }
}

/// <summary>A single risk assessment row rendered within the MOC detail page's Risk Assessments tab.</summary>
public sealed class MocRiskAssessmentRowViewModel
{
    /// <summary>Gets the risk assessment identifier.</summary>
    public required Guid RiskAssessmentId { get; init; }

    /// <summary>Gets the assessed risk level.</summary>
    public required string RiskLevel { get; init; }

    /// <summary>Gets the raw assessment status code.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the UTC timestamp when the assessment was performed.</summary>
    public required DateTime AssessedAtUtc { get; init; }
}

/// <summary>Full approval-workflow-style detail view model for a single MOC change request.</summary>
public sealed class MocDetailViewModel
{
    /// <summary>Gets the workflow identifier.</summary>
    public required Guid WorkflowId { get; init; }

    /// <summary>Gets the change title.</summary>
    public required string ChangeTitle { get; init; }

    /// <summary>Gets the current workflow stage.</summary>
    public required string CurrentStage { get; init; }

    /// <summary>Gets the raw workflow status code.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the UTC timestamp when the workflow was initiated.</summary>
    public required DateTime InitiatedAtUtc { get; init; }

    /// <summary>Gets the identifier of the user who initiated the workflow.</summary>
    public required Guid InitiatedBy { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the approval decisions recorded against this change request.</summary>
    public IReadOnlyList<MocApprovalRowViewModel> Approvals { get; init; } = [];

    /// <summary>Gets the risk assessments performed against this change request.</summary>
    public IReadOnlyList<MocRiskAssessmentRowViewModel> RiskAssessments { get; init; } = [];
}
