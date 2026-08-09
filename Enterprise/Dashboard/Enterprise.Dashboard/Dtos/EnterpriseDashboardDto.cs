// ============================================================================
//  Project:        Enterprise.Dashboard
//  Module:         Dtos
//  File:           EnterpriseDashboardDto.cs
//  Created:        2026-08-08
//  Description:    Phase D.4: the top-level enterprise dashboard payload,
//                  aggregating every sub-summary Path D's spec requires
//                  (facilities/employees/machines/hazards/severity/
//                  workflows/predictions). Built exclusively via
//                  EnterpriseDashboardProfile.Build.
// ============================================================================
namespace Enterprise.Dashboard.Dtos;

/// <summary>The top-level enterprise dashboard payload.</summary>
public sealed record EnterpriseDashboardDto(
    EnterpriseRiskSummaryDto RiskSummary,
    EnterpriseComplianceSummaryDto ComplianceSummary,
    EnterpriseHazardDistributionDto HazardDistribution,
    EnterpriseSeverityDistributionDto SeverityDistribution,
    EnterprisePredictiveAlertsDto PredictiveAlerts,
    EnterpriseWorkflowStatusDto WorkflowStatus,
    int TotalEmployees,
    int TotalMachines,
    DateTime GeneratedAtUtc);
