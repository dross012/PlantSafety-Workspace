// ============================================================================
//  Project:        Enterprise.Dashboard
//  Module:         Dtos
//  File:           EnterpriseWorkflowStatusDto.cs
//  Created:        2026-08-08
//  Description:    Phase D.4: a coarse workflow-activity summary for the
//                  dashboard, built purely from Enterprise.DataLake
//                  Workflow record counts/timestamps.
// ============================================================================
namespace Enterprise.Dashboard.Dtos;

/// <summary>A coarse workflow-activity summary derived from enterprise data lake Workflow records.</summary>
public sealed record EnterpriseWorkflowStatusDto(
    int TotalWorkflowRecords,
    DateTime? MostRecentWorkflowRecordAtUtc);
