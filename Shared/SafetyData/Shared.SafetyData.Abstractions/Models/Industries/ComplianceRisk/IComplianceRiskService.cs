// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.ComplianceRisk
//  Layer:          Abstractions
//  File:           IComplianceRiskService.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI-friendly abstraction over ComplianceRiskEngine, so
//                  consumers can take a constructor dependency instead of a
//                  static reference -- mirrors IFacilityProfileService/
//                  IEventRoutingService/IEventImpactService/
//                  IEventWorkflowService's role for their own subsystems
//                  exactly.
//                  Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

/// <summary>DI-resolvable facade over <see cref="ComplianceRiskEngine"/>.</summary>
public interface IComplianceRiskService
{
    /// <summary>Computes the given facility's compliance score.</summary>
    ComplianceScore ComputeComplianceScore(FacilityProfile facility);

    /// <summary>Computes the given facility's multi-dimensional risk score.</summary>
    RiskScore ComputeRiskScore(FacilityProfile facility);

    /// <summary>Computes the compliance trend between two score snapshots for the same facility.</summary>
    ComplianceTrend ComputeComplianceTrend(ComplianceScore previous, ComplianceScore current);

    /// <summary>Computes the risk trend between two score snapshots for the same facility.</summary>
    RiskTrend ComputeRiskTrend(RiskScore previous, RiskScore current);

    /// <summary>Computes the given facility's compliance-dashboard metadata, optionally incorporating a trend.</summary>
    ComplianceDashboardMetadata ComputeComplianceDashboard(FacilityProfile facility, ComplianceTrend? trend = null);

    /// <summary>Computes the given facility's risk-dashboard metadata, optionally incorporating a trend.</summary>
    RiskDashboardMetadata ComputeRiskDashboard(FacilityProfile facility, RiskTrend? trend = null);

    /// <summary>Computes the given facility's compliance and risk readiness.</summary>
    (ComplianceReadiness ComplianceReadiness, RiskReadiness RiskReadiness) ComputeReadiness(FacilityProfile facility);
}
