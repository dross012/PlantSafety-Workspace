// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.ComplianceRisk
//  Layer:          Abstractions
//  File:           ComplianceDashboardMetadata.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic compliance-dashboard metadata derived from
//                  a facility's FacilityComplianceProfile, ComplianceScore,
//                  and (optionally, since a first-ever assessment has no
//                  prior snapshot to compare against) ComplianceTrend.
//                  Reuses the existing Facilities.FacilityKpiIndicator shape
//                  rather than inventing a new KPI type. Additive-only
//                  evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

/// <summary>
/// Deterministic compliance-dashboard metadata for a single facility.
/// </summary>
/// <param name="FacilityId">The facility this metadata was computed for.</param>
/// <param name="KeyIndicators">Compliance KPIs, derived from <see cref="ComplianceScore"/> and, if supplied, <see cref="ComplianceTrend"/>.</param>
/// <param name="RequiredWidgets">Always includes the compliance score and tier widgets.</param>
/// <param name="RequiredCharts">Always includes a compliance trend chart.</param>
/// <param name="RequiredAlerts">One alert per missing Program-kind compliance element (the most severe gap kind).</param>
/// <param name="Trend">The compliance trend supplied to <see cref="ComplianceDashboardMetadataBuilder.Build"/>, or <see langword="null"/> if none was available.</param>
public sealed record ComplianceDashboardMetadata(
    Guid FacilityId,
    IReadOnlyList<FacilityKpiIndicator> KeyIndicators,
    IReadOnlyList<string> RequiredWidgets,
    IReadOnlyList<string> RequiredCharts,
    IReadOnlyList<string> RequiredAlerts,
    ComplianceTrend? Trend);

/// <summary>Builds <see cref="ComplianceDashboardMetadata"/> deterministically from a <see cref="FacilityProfile"/>.</summary>
public static class ComplianceDashboardMetadataBuilder
{
    private const string ComplianceScoreWidget = "ComplianceScoreWidget";
    private const string ComplianceTierWidget = "ComplianceTierWidget";
    private const string ComplianceTrendChart = "ComplianceTrendChart";

    /// <summary>
    /// Builds the compliance-dashboard metadata for the given facility. <paramref name="trend"/>
    /// is optional -- omit it when no prior <see cref="ComplianceScore"/> snapshot exists to
    /// compare against.
    /// </summary>
    public static ComplianceDashboardMetadata Build(FacilityProfile facility, ComplianceTrend? trend = null)
    {
        ComplianceScore score = ComplianceScoreBuilder.Build(facility);

        var keyIndicators = new List<FacilityKpiIndicator>
        {
            new("PercentComplete", score.PercentComplete.ToString("F1")),
            new("ComplianceTier", score.ComplianceTier.ToString()),
            new("RequiredElementsCount", score.RequiredElementsCount.ToString()),
            new("CompletedElementsCount", score.CompletedElementsCount.ToString()),
            new("MissingElementsCount", score.MissingElementsCount.ToString()),
        };

        if (trend is ComplianceTrend t)
        {
            keyIndicators.Add(new FacilityKpiIndicator("ComplianceTrendDirection", t.Direction.ToString()));
            keyIndicators.Add(new FacilityKpiIndicator("ComplianceTrendDelta", t.Delta.ToString("F1")));
        }

        IReadOnlyList<string> requiredWidgets = new[] { ComplianceScoreWidget, ComplianceTierWidget };
        IReadOnlyList<string> requiredCharts = new[] { ComplianceTrendChart };

        IReadOnlyList<string> requiredAlerts = facility.ComplianceProfile.MissingElements
            .Where(element => element.Kind == Facilities.ComplianceElementKind.Program)
            .Select(element => $"Missing required program: {element.Description}")
            .ToArray();

        return new ComplianceDashboardMetadata(facility.FacilityId, keyIndicators, requiredWidgets, requiredCharts, requiredAlerts, trend);
    }
}
