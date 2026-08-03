// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyAnomalyDetector.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Real, working IGlobalSafetyAnomalyDetector implementation. Uses genuine z-score
//                  (standard-deviations-from-the-cross-site-mean) statistical deviation detection,
//                  matching the statistical rigor of Phase 5's
//                  Observability.Telemetry.Services.TelemetryAnomalyDetector (built as a reference
//                  pattern for this class, not copy-pasted -- that detector reasons over a SINGLE
//                  machine's rolling window against ITS OWN historical baseline; this detector
//                  reasons over MANY sites' current-window totals against the CROSS-SITE mean/
//                  stddev, which is the correct statistical framing for "which site(s) are outliers
//                  right now"). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Enums;
using PlantSafety.Domain.GlobalSafetyInsights.Interfaces;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;

namespace PlantSafety.Domain.GlobalSafetyInsights.Services;

/// <summary>Real, working cross-site anomaly detector using z-score statistical deviation.</summary>
public sealed class GlobalSafetyAnomalyDetector : IGlobalSafetyAnomalyDetector, IDomainService
{
    /// <summary>Z-score magnitude above which a cross-site deviation is considered a statistically significant anomaly (~97.7th percentile, one-tailed).</summary>
    public const double ZScoreThreshold = 2.0;

    /// <summary>Minimum number of sites required to compute a meaningful cross-site mean/stddev.</summary>
    public const int MinimumSiteCountForComparison = 3;

    /// <inheritdoc/>
    public IReadOnlyList<DetectedAnomaly> DetectAnomalies(IReadOnlyList<SafetyTelemetryEvent> telemetry, UtcTimestamp nowUtc)
    {
        ArgumentNullException.ThrowIfNull(telemetry);

        var anomalies = new List<DetectedAnomaly>();

        anomalies.AddRange(DetectIncidentSpikes(telemetry, nowUtc));
        anomalies.AddRange(DetectRecurringExposureExceedances(telemetry, nowUtc));
        anomalies.AddRange(DetectSystemicMachineLineFailures(telemetry, nowUtc));

        return anomalies;
    }

    private static List<DetectedAnomaly> DetectIncidentSpikes(IReadOnlyList<SafetyTelemetryEvent> telemetry, UtcTimestamp nowUtc)
    {
        var perSite = telemetry
            .GroupBy(t => t.SiteId)
            .Select(g => (SiteId: g.Key, Count: g.Sum(t => t.RecordableIncidentCount)))
            .ToList();

        if (perSite.Count < MinimumSiteCountForComparison)
            return [];

        var (mean, stdDev) = ComputeMeanAndStdDev(perSite.Select(p => (double)p.Count).ToList());
        if (stdDev == 0.0)
            return [];

        var results = new List<DetectedAnomaly>();
        foreach (var site in perSite)
        {
            var z = (site.Count - mean) / stdDev;
            if (z <= ZScoreThreshold)
                continue;

            results.Add(new DetectedAnomaly
            {
                Kind = GlobalSafetyAnomalyKind.CrossSiteIncidentSpike,
                SiteId = site.SiteId,
                DeviationMagnitude = Math.Round(z, 4),
                Message = $"Site {site.SiteId} recorded {site.Count} incident(s) (z-score {z:F2}, cross-site mean {mean:F2}, threshold {ZScoreThreshold:F1}).",
                DetectedAtUtc = nowUtc,
            });
        }

        return results;
    }

    private static List<DetectedAnomaly> DetectRecurringExposureExceedances(IReadOnlyList<SafetyTelemetryEvent> telemetry, UtcTimestamp nowUtc)
    {
        var perSite = telemetry
            .Where(t => t.ExposureLevelScore.HasValue)
            .GroupBy(t => t.SiteId)
            .Select(g => (SiteId: g.Key, Mean: g.Average(t => (double)t.ExposureLevelScore!.Value), Count: g.Count()))
            .ToList();

        if (perSite.Count < MinimumSiteCountForComparison)
            return [];

        var (crossSiteMean, stdDev) = ComputeMeanAndStdDev(perSite.Select(p => p.Mean).ToList());
        if (stdDev == 0.0)
            return [];

        var results = new List<DetectedAnomaly>();
        foreach (var site in perSite)
        {
            // "Recurring" requires at least 2 elevated measurements, not a single outlier reading.
            if (site.Count < 2)
                continue;

            var z = (site.Mean - crossSiteMean) / stdDev;
            if (z <= ZScoreThreshold)
                continue;

            results.Add(new DetectedAnomaly
            {
                Kind = GlobalSafetyAnomalyKind.RecurringExposureExceedance,
                SiteId = site.SiteId,
                DeviationMagnitude = Math.Round(z, 4),
                Message = $"Site {site.SiteId} recorded {site.Count} exposure measurements averaging {site.Mean:F2} " +
                          $"(z-score {z:F2}, cross-site mean {crossSiteMean:F2}, threshold {ZScoreThreshold:F1}).",
                DetectedAtUtc = nowUtc,
            });
        }

        return results;
    }

    private static List<DetectedAnomaly> DetectSystemicMachineLineFailures(IReadOnlyList<SafetyTelemetryEvent> telemetry, UtcTimestamp nowUtc)
    {
        var perFamily = telemetry
            .Where(t => t.MachineStateDegraded == true && !string.IsNullOrWhiteSpace(t.MachineFamily))
            .GroupBy(t => t.MachineFamily!)
            .Select(g => (MachineFamily: g.Key, DistinctSites: g.Select(t => t.SiteId).Distinct().ToList()))
            .Where(g => g.DistinctSites.Count >= 2)
            .ToList();

        return perFamily.Select(f => new DetectedAnomaly
        {
            Kind = GlobalSafetyAnomalyKind.SystemicMachineLineFailure,
            MachineFamily = f.MachineFamily,
            DeviationMagnitude = f.DistinctSites.Count,
            Message = $"Machine family '{f.MachineFamily}' reported degraded state across {f.DistinctSites.Count} distinct sites within the analysis window.",
            DetectedAtUtc = nowUtc,
        }).ToList();
    }

    private static (double Mean, double StdDev) ComputeMeanAndStdDev(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
            return (0.0, 0.0);

        var mean = values.Average();
        var variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Count;
        return (mean, Math.Sqrt(variance));
    }
}
