// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: UI-layer mapping service projecting MOC read-model DTOs
//   into display-ready view models. Named and shaped after the platform's
//   standard Mapster DTO -> ViewModel convention (Adapt-style pure static
//   projections, no side effects); implemented as hand-written static
//   methods rather than the Mapster package (see
//   Modules/PlanningProjects/Mappings/PlanningProjectsMappingService.cs
//   for the precedent establishing this convention).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.MOC.Models;

namespace PlantSafety.UI.Modules.MOC.Mappings;

/// <summary>Pure, stateless DTO -&gt; ViewModel projections for the Management of Change module.</summary>
public static class MocMappingService
{
    /// <summary>Projects a workflow item into a list-row view model.</summary>
    public static MocRowViewModel ToRowViewModel(this MocWorkflowItemDto dto) => new()
    {
        WorkflowId = dto.WorkflowId,
        ChangeTitle = dto.ChangeTitle,
        CurrentStage = dto.CurrentStage,
        Status = dto.Status,
        StatusIndicatorState = ToStatusIndicatorState(dto.Status),
        InitiatedAtUtc = dto.InitiatedAtUtc,
    };

    /// <summary>Projects a list of workflow items into list-row view models.</summary>
    public static IReadOnlyList<MocRowViewModel> ToRowViewModels(this IEnumerable<MocWorkflowItemDto> items) =>
        items.Select(ToRowViewModel).ToList();

    /// <summary>Projects a workflow item, plus its correlated approvals/risk assessments, into a full detail view model.</summary>
    /// <param name="dto">The workflow item.</param>
    /// <param name="allApprovals">The full tenant approvals result set; filtered by ChangeRequestId == WorkflowId.</param>
    /// <param name="allRiskAssessments">The full tenant risk-assessment result set; filtered by ChangeRequestId == WorkflowId.</param>
    public static MocDetailViewModel ToDetailViewModel(
        this MocWorkflowItemDto dto,
        IEnumerable<MocApprovalItemDto> allApprovals,
        IEnumerable<MocRiskAssessmentItemDto> allRiskAssessments) => new()
    {
        WorkflowId = dto.WorkflowId,
        ChangeTitle = dto.ChangeTitle,
        CurrentStage = dto.CurrentStage,
        Status = dto.Status,
        StatusIndicatorState = ToStatusIndicatorState(dto.Status),
        InitiatedAtUtc = dto.InitiatedAtUtc,
        InitiatedBy = dto.InitiatedBy,
        TenantId = dto.TenantId,
        SiteId = dto.SiteId,
        Approvals = allApprovals
            .Where(a => a.ChangeRequestId == dto.WorkflowId)
            .Select(a => new MocApprovalRowViewModel
            {
                ApprovalId = a.ApprovalId,
                ApproverName = a.ApproverName,
                Status = a.Status,
                StatusIndicatorState = ToStatusIndicatorState(a.Status),
                DecidedAtUtc = a.DecidedAtUtc,
            })
            .ToList(),
        RiskAssessments = allRiskAssessments
            .Where(r => r.ChangeRequestId == dto.WorkflowId)
            .Select(r => new MocRiskAssessmentRowViewModel
            {
                RiskAssessmentId = r.RiskAssessmentId,
                RiskLevel = r.RiskLevel,
                Status = r.Status,
                StatusIndicatorState = ToStatusIndicatorState(r.Status),
                AssessedAtUtc = r.AssessedAtUtc,
            })
            .ToList(),
    };

    /// <summary>Maps a raw MOC status code to the closest StatusIndicatorComponent operational state.</summary>
    /// <param name="status">The raw status code (case-insensitive; covers Workflow/Approval/RiskAssessment vocabularies).</param>
    public static string ToStatusIndicatorState(string status) => status.Trim().ToLowerInvariant() switch
    {
        "completed" or "approved" or "finalized" => "Healthy",
        "open" or "pending" or "draft" or "inreview" or "onhold" => "Warning",
        "rejected" or "critical" or "high" => "Critical",
        "cancelled" => "Offline",
        _ => "Offline",
    };
}
