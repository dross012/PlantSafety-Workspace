// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.ComplianceRisk
//  Layer:          Abstractions
//  File:           RiskDashboardMetadata.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic risk-dashboard metadata derived from a
//                  facility's FacilityProfile, RiskScore, and (optionally,
//                  since a first-ever assessment has no prior snapshot to
//                  compare against) RiskTrend. Reuses the existing
//                  Facilities.FacilityKpiIndicator shape rather than
//                  inventing a new KPI type. Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

/// <summary>
/// Deterministic risk-dashboard metadata for a single facility.
/// </summary>
/// <param name="FacilityId">The facility this metadata was computed for.</param>
/// <param name="KeyIndicators">Risk KPIs, derived from <see cref="RiskScore"/> and, if supplied, <see cref="RiskTrend"/>.</param>
/// <param name="RequiredWidgets">Always includes the overall-risk widget, plus one per dimension (hazard/program/regulator/event) currently above <see cref="RiskLevel.Low"/>.</param>
/// <param name="RequiredCharts">Always includes a risk trend chart.</param>
/// <param name="RequiredAlerts">One alert per risk dimension currently at <see cref="RiskLevel.High"/> or <see cref="RiskLevel.Critical"/>.</param>
/// <param name="Trend">The risk trend supplied to <see cref="RiskDashboardMetadataBuilder.Build"/>, or <see langword="null"/> if none was available.</param>
public sealed record RiskDashboardMetadata(
    Guid FacilityId,
    IReadOnlyList<FacilityKpiIndicator> KeyIndicators,
    IReadOnlyList<string> RequiredWidgets,
    IReadOnlyList<string> RequiredCharts,
    IReadOnlyList<string> RequiredAlerts,
    RiskTrend? Trend);

/// <summary>Builds <see cref="RiskDashboardMetadata"/> deterministically from a <see cref="FacilityProfile"/>.</summary>
public static class RiskDashboardMetadataBuilder
{
    private const string OverallRiskWidget = "OverallRiskWidget";
    private const string RiskTrendChart = "RiskTrendChart";

    /// <summary>
    /// Builds the risk-dashboard metadata for the given facility. <paramref name="trend"/> is
    /// optional -- omit it when no prior <see cref="RiskScore"/> snapshot exists to compare
    /// against.
    /// </summary>
    public static RiskDashboardMetadata Build(FacilityProfile facility, RiskTrend? trend = null)
    {
        RiskScore score = RiskScoreBuilder.Build(facility);

        var dimensions = new (string Name, RiskLevel Level)[]
        {
            ("Hazard", score.HazardRiskLevel),
            ("Program", score.ProgramRiskLevel),
            ("Regulator", score.RegulatorRiskLevel),
            ("Event", score.EventRiskLevel),
        };

        var keyIndicators = new List<FacilityKpiIndicator>
        {
            new("OverallRiskLevel", score.OverallRiskLevel.ToString()),
            new("HazardRiskLevel", score.HazardRiskLevel.ToString()),
            new("ProgramRiskLevel", score.ProgramRiskLevel.ToString()),
            new("RegulatorRiskLevel", score.RegulatorRiskLevel.ToString()),
            new("EventRiskLevel", score.EventRiskLevel.ToString()),
        };

        if (trend is RiskTrend t)
        {
            keyIndicators.Add(new FacilityKpiIndicator("RiskTrendDirection", t.Direction.ToString()));
            keyIndicators.Add(new FacilityKpiIndicator("RiskTrendDelta", t.Delta.ToString()));
        }

        IReadOnlyList<string> requiredWidgets = new[] { OverallRiskWidget }
            .Concat(dimensions.Where(d => d.Level > RiskLevel.Low).Select(d => $"{d.Name}RiskWidget"))
            .ToArray();

        IReadOnlyList<string> requiredCharts = new[] { RiskTrendChart };

        IReadOnlyList<string> requiredAlerts = dimensions
            .Where(d => d.Level is RiskLevel.High or RiskLevel.Critical)
            .Select(d => $"{d.Name} risk is {d.Level}.")
            .ToArray();

        return new RiskDashboardMetadata(facility.FacilityId, keyIndicators, requiredWidgets, requiredCharts, requiredAlerts, trend);
    }
}
