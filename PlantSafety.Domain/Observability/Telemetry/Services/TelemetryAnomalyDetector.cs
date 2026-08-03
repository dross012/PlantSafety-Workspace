// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.Telemetry
//  Layer:          Domain
//  File:           TelemetryAnomalyDetector.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Domain service that inspects a rolling window of production telemetry
//                  (cycle times, downtime events, quality readings) and flags statistically
//                  significant anomalies for the Application-layer event stream processor to
//                  route into alerts/dashboards. Pure computation only. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.Observability.Telemetry.ValueObjects;

namespace PlantSafety.Domain.Observability.Telemetry.Services;

/// <summary>Category of anomaly raised by <see cref="TelemetryAnomalyDetector"/>.</summary>
public enum TelemetryAnomalyKind
{
    /// <summary>Observed cycle time significantly exceeds the ideal/rated cycle time.</summary>
    CycleTimeDegradation = 0,

    /// <summary>Downtime events are recurring at a significantly elevated frequency.</summary>
    DowntimeFrequencySpike = 1,

    /// <summary>Scrap/defect rate has drifted significantly above the historical baseline.</summary>
    QualityDrift = 2,
}

/// <summary>A single anomaly flagged by the telemetry anomaly detector.</summary>
public sealed record TelemetryAnomaly
{
    /// <summary>Identity of the machine the anomaly pertains to.</summary>
    public required Guid MachineId { get; init; }

    /// <summary>Category of the anomaly.</summary>
    public required TelemetryAnomalyKind Kind { get; init; }

    /// <summary>Magnitude of the deviation from baseline, expressed as a ratio (e.g. 1.5 = 50% above baseline).</summary>
    public required double DeviationRatio { get; init; }

    /// <summary>Human-readable explanation of the anomaly.</summary>
    public required string Message { get; init; }
}

/// <summary>Detects statistically significant anomalies in a rolling window of production telemetry.</summary>
public sealed class TelemetryAnomalyDetector : IDomainService
{
    /// <summary>Cycle time ratio above which a CycleTimeDegradation anomaly is raised.</summary>
    public const double CycleTimeDegradationThreshold = 1.25;

    /// <summary>Downtime event count per hour above which a DowntimeFrequencySpike anomaly is raised.</summary>
    public const double DowntimeEventsPerHourThreshold = 2.0;

    /// <summary>Scrap-rate deviation ratio above which a QualityDrift anomaly is raised.</summary>
    public const double QualityDriftThreshold = 1.5;

    /// <summary>Evaluates the most recent cycle time reading for a CycleTimeDegradation anomaly.</summary>
    public TelemetryAnomaly? DetectCycleTimeAnomaly(CycleTimeReading reading)
    {
        ArgumentNullException.ThrowIfNull(reading);

        if (reading.CycleTimeRatio <= CycleTimeDegradationThreshold)
            return null;

        return new TelemetryAnomaly
        {
            MachineId = reading.MachineId,
            Kind = TelemetryAnomalyKind.CycleTimeDegradation,
            DeviationRatio = Math.Round(reading.CycleTimeRatio, 4),
            Message = $"Observed cycle time is {reading.CycleTimeRatio:P0} of ideal cycle time " +
                      $"(threshold {CycleTimeDegradationThreshold:P0}).",
        };
    }

    /// <summary>Evaluates a rolling window of downtime events for a DowntimeFrequencySpike anomaly.</summary>
    public TelemetryAnomaly? DetectDowntimeFrequencyAnomaly(
        Guid machineId, IReadOnlyList<DowntimeEvent> events, TimeSpan window)
    {
        ArgumentNullException.ThrowIfNull(events);
        if (window <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(window));

        var machineEvents = events.Where(e => e.MachineId == machineId).ToList();
        var eventsPerHour = machineEvents.Count / window.TotalHours;

        if (eventsPerHour <= DowntimeEventsPerHourThreshold)
            return null;

        return new TelemetryAnomaly
        {
            MachineId = machineId,
            Kind = TelemetryAnomalyKind.DowntimeFrequencySpike,
            DeviationRatio = Math.Round(eventsPerHour / DowntimeEventsPerHourThreshold, 4),
            Message = $"{machineEvents.Count} downtime event(s) in {window.TotalHours:F1}h " +
                      $"({eventsPerHour:F2}/h, threshold {DowntimeEventsPerHourThreshold:F2}/h).",
        };
    }

    /// <summary>Evaluates a scrap-rate reading against a historical baseline for a QualityDrift anomaly.</summary>
    public TelemetryAnomaly? DetectQualityDriftAnomaly(
        ProductionCountReading reading, double baselineScrapRate)
    {
        ArgumentNullException.ThrowIfNull(reading);
        if (baselineScrapRate < 0)
            throw new ArgumentOutOfRangeException(nameof(baselineScrapRate));

        if (reading.TotalUnitCount == 0)
            return null;

        var currentScrapRate = reading.ScrapUnitCount / (double)reading.TotalUnitCount;

        if (baselineScrapRate <= 0)
            return currentScrapRate > 0
                ? new TelemetryAnomaly
                {
                    MachineId = reading.MachineId,
                    Kind = TelemetryAnomalyKind.QualityDrift,
                    DeviationRatio = double.PositiveInfinity,
                    Message = $"Scrap rate of {currentScrapRate:P1} observed against a zero baseline.",
                }
                : null;

        var deviationRatio = currentScrapRate / baselineScrapRate;

        if (deviationRatio <= QualityDriftThreshold)
            return null;

        return new TelemetryAnomaly
        {
            MachineId = reading.MachineId,
            Kind = TelemetryAnomalyKind.QualityDrift,
            DeviationRatio = Math.Round(deviationRatio, 4),
            Message = $"Scrap rate {currentScrapRate:P1} is {deviationRatio:F2}x the baseline " +
                      $"{baselineScrapRate:P1} (threshold {QualityDriftThreshold:F2}x).",
        };
    }
}
