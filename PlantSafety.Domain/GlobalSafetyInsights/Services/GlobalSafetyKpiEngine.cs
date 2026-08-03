// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyKpiEngine.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Real, working IGlobalSafetyKpiEngine implementation. TRIR/LTIR REUSE the exact
//                  OSHA-standard incidence-rate formula already implicit in the platform (TRIR =
//                  Total Recordable Incidents x 200,000 / Total Hours Worked; LTIR = Lost Time
//                  Incidents x 200,000 / Total Hours Worked -- the 200,000-hour base represents 100
//                  full-time employees working 40 hours/week, 50 weeks/year, per 29 CFR 1904/OSHA
//                  Recordkeeping). This is a NEW implementation, not a reuse of an existing
//                  calculator class: investigation of
//                  PlantSafety.Application.Analytics.Dashboard.Executive (ExecutiveItemDto/
//                  IExecutiveRepository) and PlantSafety.Application.Scorecards.LaggingIndicators
//                  (LaggingIndicatorsItemDto, "TRIR, DART, Lost Workdays") confirmed BOTH are
//                  generic read-model DTO shells (MetricName/MetricValue string/decimal pairs) with
//                  NO backing Infrastructure repository implementation and NO formula anywhere in
//                  the codebase -- there is no existing TRIR/LTIR calculator class to reuse (unlike
//                  Analytics.Dashboard.Kpi.Services.OeeCalculator, which IS a real, reusable
//                  formula this engine's placement convention mirrors). The formula itself is the
//                  single universally standard OSHA definition, so "new" here means "first real
//                  implementation of the standard formula", not "a competing, divergent formula".
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Interfaces;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;

namespace PlantSafety.Domain.GlobalSafetyInsights.Services;

/// <summary>Real, working cross-site safety KPI engine.</summary>
public sealed class GlobalSafetyKpiEngine : IGlobalSafetyKpiEngine, IDomainService
{
    /// <summary>OSHA 29 CFR 1904 standard incidence-rate base: 100 full-time employees x 2,000 hours/year.</summary>
    public const decimal OshaIncidenceRateBase = 200_000m;

    /// <inheritdoc/>
    public GlobalSafetyKpiSnapshot Compute(IReadOnlyList<SafetyTelemetryEvent> telemetry)
    {
        ArgumentNullException.ThrowIfNull(telemetry);

        var totalRecordable = telemetry.Sum(t => t.RecordableIncidentCount);
        var totalLostTime = telemetry.Sum(t => t.LostTimeIncidentCount);
        var totalHoursWorked = telemetry.Sum(t => t.HoursWorkedContribution ?? 0m);

        var trir = totalHoursWorked > 0
            ? Math.Round(totalRecordable * OshaIncidenceRateBase / totalHoursWorked, 4)
            : 0m;

        var ltir = totalHoursWorked > 0
            ? Math.Round(totalLostTime * OshaIncidenceRateBase / totalHoursWorked, 4)
            : 0m;

        var exposureReadings = telemetry.Where(t => t.ExposureLevelScore.HasValue).Select(t => t.ExposureLevelScore!.Value).ToList();
        var exposureRate = exposureReadings.Count > 0 ? Math.Round(exposureReadings.Average(), 4) : 0m;

        var distinctSites = telemetry.Select(t => t.SiteId).Distinct().Count();
        var windowDays = ComputeWindowDays(telemetry);
        var incidentDensity = distinctSites > 0 && windowDays > 0
            ? Math.Round(totalRecordable / (distinctSites * windowDays), 6)
            : 0m;

        var correlation = ComputeMachineStateIncidentCorrelation(telemetry);

        return new GlobalSafetyKpiSnapshot
        {
            Trir = trir,
            Ltir = ltir,
            ExposureRate = exposureRate,
            IncidentDensity = incidentDensity,
            MachineStateIncidentCorrelation = correlation,
            TotalRecordableIncidents = totalRecordable,
            TotalLostTimeIncidents = totalLostTime,
            TotalHoursWorked = totalHoursWorked,
        };
    }

    private static decimal ComputeWindowDays(IReadOnlyList<SafetyTelemetryEvent> telemetry)
    {
        if (telemetry.Count == 0)
            return 0m;

        var min = telemetry.Min(t => t.OccurredAtUtc.Value);
        var max = telemetry.Max(t => t.OccurredAtUtc.Value);
        var days = (decimal)(max - min).TotalDays;
        return days < 1m ? 1m : days;
    }

    /// <summary>
    /// Pearson correlation coefficient between each site's degraded-machine-state observation
    /// count and its recordable-incident count. Returns 0 when fewer than 2 sites or zero variance
    /// (an undefined/degenerate correlation, never a fabricated non-zero value).
    /// </summary>
    private static double ComputeMachineStateIncidentCorrelation(IReadOnlyList<SafetyTelemetryEvent> telemetry)
    {
        var perSite = telemetry
            .GroupBy(t => t.SiteId)
            .Select(g => (
                Degraded: (double)g.Count(t => t.MachineStateDegraded == true),
                Incidents: (double)g.Sum(t => t.RecordableIncidentCount)))
            .ToList();

        if (perSite.Count < 2)
            return 0.0;

        var meanDegraded = perSite.Average(p => p.Degraded);
        var meanIncidents = perSite.Average(p => p.Incidents);

        var covariance = perSite.Sum(p => (p.Degraded - meanDegraded) * (p.Incidents - meanIncidents));
        var varDegraded = Math.Sqrt(perSite.Sum(p => Math.Pow(p.Degraded - meanDegraded, 2)));
        var varIncidents = Math.Sqrt(perSite.Sum(p => Math.Pow(p.Incidents - meanIncidents, 2)));

        if (varDegraded == 0.0 || varIncidents == 0.0)
            return 0.0;

        var correlation = covariance / (varDegraded * varIncidents);
        return Math.Round(Math.Clamp(correlation, -1.0, 1.0), 4);
    }
}
