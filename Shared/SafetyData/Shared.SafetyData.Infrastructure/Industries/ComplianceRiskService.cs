// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Industries
//  Layer:          Infrastructure
//  File:           ComplianceRiskService.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Stateless IComplianceRiskService implementation delegating
//                  to the static ComplianceRiskEngine (Abstractions.Models.
//                  Industries.ComplianceRisk). Mirrors FacilityProfileService/
//                  EventRoutingService/EventImpactService/EventWorkflowService
//                  exactly. Stateless, so a single instance is safe to share
//                  as a singleton.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Infrastructure.Industries;

/// <summary>Stateless <see cref="IComplianceRiskService"/> implementation backed by <see cref="ComplianceRiskEngine"/>.</summary>
public sealed class ComplianceRiskService : IComplianceRiskService
{
    /// <inheritdoc />
    public ComplianceScore ComputeComplianceScore(FacilityProfile facility) =>
        ComplianceRiskEngine.ComputeComplianceScore(facility);

    /// <inheritdoc />
    public RiskScore ComputeRiskScore(FacilityProfile facility) =>
        ComplianceRiskEngine.ComputeRiskScore(facility);

    /// <inheritdoc />
    public ComplianceTrend ComputeComplianceTrend(ComplianceScore previous, ComplianceScore current) =>
        ComplianceRiskEngine.ComputeComplianceTrend(previous, current);

    /// <inheritdoc />
    public RiskTrend ComputeRiskTrend(RiskScore previous, RiskScore current) =>
        ComplianceRiskEngine.ComputeRiskTrend(previous, current);

    /// <inheritdoc />
    public ComplianceDashboardMetadata ComputeComplianceDashboard(FacilityProfile facility, ComplianceTrend? trend = null) =>
        ComplianceRiskEngine.ComputeComplianceDashboard(facility, trend);

    /// <inheritdoc />
    public RiskDashboardMetadata ComputeRiskDashboard(FacilityProfile facility, RiskTrend? trend = null) =>
        ComplianceRiskEngine.ComputeRiskDashboard(facility, trend);

    /// <inheritdoc />
    public (ComplianceReadiness ComplianceReadiness, RiskReadiness RiskReadiness) ComputeReadiness(FacilityProfile facility) =>
        ComplianceRiskEngine.ComputeReadiness(facility);
}
