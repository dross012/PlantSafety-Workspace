// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Facilities
//  Layer:          Abstractions
//  File:           FacilityDashboardMetadata.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic dashboard metadata for a single facility:
//                  key indicators (from FacilityComplianceProfile), required
//                  widgets (one per hazard flag, via FacilityWidgetCatalog --
//                  a new, facility-dashboard-specific catalog since none of
//                  the existing flag-derived catalogs (SafetyModule/SopTemplate
//                  /AuditChecklistItem/HazardCategory) are UI-widget shaped),
//                  required charts, and required alerts (one per missing
//                  required Program -- the most severe compliance gap kind).
//                  No new flags/industries/programs introduced -- everything
//                  is computed from an already-built FacilityProfile.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Facilities;

/// <summary>A single dashboard key-performance-indicator, rendered as a name/value pair.</summary>
public sealed record FacilityKpiIndicator(string Name, string Value);

/// <summary>Total, 1:1 mapping from each single-bit <see cref="SafetyFlag"/> to a dashboard widget name.</summary>
public static class FacilityWidgetCatalog
{
    /// <summary>Returns the dashboard widget name corresponding to a single-bit <see cref="SafetyFlag"/> value.</summary>
    public static string WidgetForFlag(SafetyFlag flag) => flag switch
    {
        SafetyFlag.Machinery => "MachineGuardingStatusWidget",
        SafetyFlag.Chemicals => "ChemicalInventoryWidget",
        SafetyFlag.Flammables => "HotWorkPermitStatusWidget",
        SafetyFlag.HazmatShipping => "HazmatShipmentTrackerWidget",
        SafetyFlag.Fleet => "FleetSafetyWidget",
        SafetyFlag.EnvironmentalPermits => "EnvironmentalPermitStatusWidget",
        SafetyFlag.ExplosionRisk => "ExplosionRiskWidget",
        SafetyFlag.ConfinedSpaces => "ConfinedSpaceEntryWidget",
        SafetyFlag.LOTO => "LockoutTagoutStatusWidget",
        SafetyFlag.Dust => "DustControlWidget",
        _ => throw new ArgumentOutOfRangeException(nameof(flag), flag, "Unknown or non-single-bit SafetyFlag."),
    };
}

/// <summary>
/// Deterministic dashboard metadata for a single facility.
/// </summary>
/// <param name="FacilityId">The facility this metadata was computed for (<see cref="FacilityProfile.FacilityId"/>).</param>
/// <param name="KeyIndicators">Compliance-scorecard KPIs, derived from <see cref="FacilityComplianceProfile"/>.</param>
/// <param name="RequiredWidgets">One widget per hazard flag on the facility's industry, plus the always-present compliance widgets.</param>
/// <param name="RequiredCharts">Always includes a compliance trend chart; adds a risk trend chart when <see cref="ComplianceRiskLevel"/> is not <see cref="ComplianceRiskLevel.Low"/>.</param>
/// <param name="RequiredAlerts">One alert per missing required Program -- a missing program is the most severe compliance gap, since every downstream permit/training/audit/SOP for that program is also, by definition, unmet.</param>
public sealed record FacilityDashboardMetadata(
    Guid FacilityId,
    IReadOnlyList<FacilityKpiIndicator> KeyIndicators,
    IReadOnlyList<string> RequiredWidgets,
    IReadOnlyList<string> RequiredCharts,
    IReadOnlyList<string> RequiredAlerts);

/// <summary>Builds <see cref="FacilityDashboardMetadata"/> deterministically from a <see cref="FacilityProfile"/>.</summary>
public static class FacilityDashboardMetadataBuilder
{
    private const string ComplianceScoreWidget = "ComplianceScoreWidget";
    private const string MissingElementsWidget = "MissingElementsWidget";
    private const string ComplianceTrendChart = "ComplianceTrendChart";
    private const string RiskTrendChart = "RiskTrendChart";

    /// <summary>Builds the dashboard metadata for the given facility.</summary>
    public static FacilityDashboardMetadata Build(FacilityProfile facility)
    {
        FacilityComplianceProfile compliance = facility.ComplianceProfile;

        IReadOnlyList<FacilityKpiIndicator> keyIndicators = new[]
        {
            new FacilityKpiIndicator("ComplianceScorePercent", compliance.ComplianceScorePercent.ToString("F1")),
            new FacilityKpiIndicator("RiskLevel", compliance.RiskLevel.ToString()),
            new FacilityKpiIndicator("RequiredElementCount", compliance.RequiredElements.Count.ToString()),
            new FacilityKpiIndicator("CompletedElementCount", compliance.CompletedElements.Count.ToString()),
            new FacilityKpiIndicator("MissingElementCount", compliance.MissingElements.Count.ToString()),
        };

        IReadOnlyList<string> requiredWidgets = new[] { ComplianceScoreWidget, MissingElementsWidget }
            .Concat(SafetyFlagCatalog.Decompose(facility.Flags).Select(FacilityWidgetCatalog.WidgetForFlag))
            .ToArray();

        IReadOnlyList<string> requiredCharts = compliance.RiskLevel == ComplianceRiskLevel.Low
            ? new[] { ComplianceTrendChart }
            : new[] { ComplianceTrendChart, RiskTrendChart };

        IReadOnlyList<string> requiredAlerts = compliance.MissingElements
            .Where(element => element.Kind == ComplianceElementKind.Program)
            .Select(element => $"Missing required program: {element.Description}")
            .ToArray();

        return new FacilityDashboardMetadata(facility.FacilityId, keyIndicators, requiredWidgets, requiredCharts, requiredAlerts);
    }
}
