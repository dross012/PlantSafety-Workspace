// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.ComplianceRisk
//  Layer:          Abstractions
//  File:           ComplianceRiskEngine.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic computation entry point for the Compliance
//                  & Risk subsystem. Every method is a thin pass-through to
//                  the corresponding builder (ComplianceScoreBuilder/
//                  RiskScoreBuilder/ComplianceTrendBuilder/RiskTrendBuilder/
//                  ComplianceDashboardMetadataBuilder/
//                  RiskDashboardMetadataBuilder/ComplianceReadinessBuilder/
//                  RiskReadinessBuilder) -- this type exists to give the
//                  subsystem a single, discoverable static entry point (and
//                  the shape IComplianceRiskService below mirrors for DI),
//                  not to hold any logic of its own.
//                  Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

/// <summary>Deterministic computation entry point for the Compliance &amp; Risk subsystem.</summary>
public static class ComplianceRiskEngine
{
    /// <summary>Computes the given facility's compliance score (<see cref="ComplianceScoreBuilder.Build"/>).</summary>
    public static ComplianceScore ComputeComplianceScore(FacilityProfile facility) =>
        ComplianceScoreBuilder.Build(facility);

    /// <summary>Computes the given facility's multi-dimensional risk score (<see cref="RiskScoreBuilder.Build"/>).</summary>
    public static RiskScore ComputeRiskScore(FacilityProfile facility) =>
        RiskScoreBuilder.Build(facility);

    /// <summary>Computes the compliance trend between two score snapshots (<see cref="ComplianceTrendBuilder.Build"/>).</summary>
    public static ComplianceTrend ComputeComplianceTrend(ComplianceScore previous, ComplianceScore current) =>
        ComplianceTrendBuilder.Build(previous, current);

    /// <summary>Computes the risk trend between two score snapshots (<see cref="RiskTrendBuilder.Build"/>).</summary>
    public static RiskTrend ComputeRiskTrend(RiskScore previous, RiskScore current) =>
        RiskTrendBuilder.Build(previous, current);

    /// <summary>Computes the given facility's compliance-dashboard metadata (<see cref="ComplianceDashboardMetadataBuilder.Build"/>).</summary>
    public static ComplianceDashboardMetadata ComputeComplianceDashboard(FacilityProfile facility, ComplianceTrend? trend = null) =>
        ComplianceDashboardMetadataBuilder.Build(facility, trend);

    /// <summary>Computes the given facility's risk-dashboard metadata (<see cref="RiskDashboardMetadataBuilder.Build"/>).</summary>
    public static RiskDashboardMetadata ComputeRiskDashboard(FacilityProfile facility, RiskTrend? trend = null) =>
        RiskDashboardMetadataBuilder.Build(facility, trend);

    /// <summary>
    /// Computes the given facility's compliance and risk readiness (<see cref="ComplianceReadinessBuilder.Build"/>
    /// and <see cref="RiskReadinessBuilder.Build"/>).
    /// </summary>
    public static (ComplianceReadiness ComplianceReadiness, RiskReadiness RiskReadiness) ComputeReadiness(FacilityProfile facility) =>
        (ComplianceReadinessBuilder.Build(facility), RiskReadinessBuilder.Build(facility));
}
