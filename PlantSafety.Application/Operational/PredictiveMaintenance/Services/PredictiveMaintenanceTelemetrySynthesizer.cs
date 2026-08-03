// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           PredictiveMaintenanceTelemetrySynthesizer.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Bridges the Sensor Data Explorer's two sub-resources (sensors/data,
//                  sensors/anomalies) onto the real Phase 5 Observability.Telemetry domain
//                  service (TelemetryAnomalyDetector) rather than fabricating a parallel
//                  detection algorithm. No telemetry ingestion pipeline is persisted anywhere in
//                  the platform yet (Observability.Telemetry is a pure in-memory domain
//                  service/value-object set with no repository) -- so the underlying readings fed
//                  into the real detector are deterministically synthesized from an equipment's
//                  own identity/alert history (seeded, not random, so results are stable per
//                  equipment) rather than measured. This is flagged to product as a placeholder
//                  pending a real sensor ingestion pipeline; the anomaly *detection logic and
//                  thresholds* exercised here are the genuine platform algorithm.
//                  See docs/Phase5/ui-gaps-scoping.md §4 risk note. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Domain.Observability.Telemetry.Services;
using PlantSafety.Domain.Observability.Telemetry.ValueObjects;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Services;

/// <summary>Synthesizes deterministic telemetry readings for an equipment and runs the real TelemetryAnomalyDetector against them.</summary>
public sealed class PredictiveMaintenanceTelemetrySynthesizer
{
    private const int WindowPointCount = 12;
    private static readonly TimeSpan PointSpacing = TimeSpan.FromHours(1);

    private readonly TelemetryAnomalyDetector _detector = new();

    /// <summary>Produces a deterministic rolling window of sensor data points for the given equipment, seeded from its identity.</summary>
    public IReadOnlyList<PredictiveMaintenanceSensorDataPointDto> GenerateSensorData(Guid equipmentId, DateTime asOfUtc, string? channelFilter)
    {
        var rng = SeededRandom(equipmentId);
        var points = new List<PredictiveMaintenanceSensorDataPointDto>();

        for (var i = WindowPointCount - 1; i >= 0; i--)
        {
            var timestamp = asOfUtc - (PointSpacing * i);

            if (channelFilter is null || channelFilter.Equals("CycleTime", StringComparison.OrdinalIgnoreCase))
            {
                points.Add(new PredictiveMaintenanceSensorDataPointDto
                {
                    ChannelName = "CycleTime",
                    TimestampUtc = timestamp,
                    Value = Math.Round(0.85 + (rng.NextDouble() * 0.6), 3),
                    Unit = "ratio-of-ideal",
                });
            }

            if (channelFilter is null || channelFilter.Equals("ScrapRate", StringComparison.OrdinalIgnoreCase))
            {
                points.Add(new PredictiveMaintenanceSensorDataPointDto
                {
                    ChannelName = "ScrapRate",
                    TimestampUtc = timestamp,
                    Value = Math.Round(rng.NextDouble() * 0.12, 4),
                    Unit = "fraction",
                });
            }

            if (channelFilter is null || channelFilter.Equals("DowntimeMinutes", StringComparison.OrdinalIgnoreCase))
            {
                points.Add(new PredictiveMaintenanceSensorDataPointDto
                {
                    ChannelName = "DowntimeMinutes",
                    TimestampUtc = timestamp,
                    Value = Math.Round(rng.NextDouble() * 15.0, 1),
                    Unit = "minutes",
                });
            }
        }

        return points;
    }

    /// <summary>
    /// Synthesizes the same deterministic window used by <see cref="GenerateSensorData"/> as real
    /// Observability.Telemetry value objects, then runs the real TelemetryAnomalyDetector against
    /// them and returns whatever anomalies the genuine detection algorithm flags.
    /// </summary>
    public IReadOnlyList<PredictiveMaintenanceSensorAnomalyDto> DetectAnomalies(Guid equipmentId, DateTime asOfUtc)
    {
        var rng = SeededRandom(equipmentId);
        var anomalies = new List<PredictiveMaintenanceSensorAnomalyDto>();

        var downtimeEvents = new List<DowntimeEvent>();

        for (var i = WindowPointCount - 1; i >= 0; i--)
        {
            var timestamp = UtcTimestamp.From(DateTime.SpecifyKind(asOfUtc - (PointSpacing * i), DateTimeKind.Utc));

            var observedRatio = 0.85 + (rng.NextDouble() * 0.6);
            var cycleReading = new CycleTimeReading
            {
                MachineId = equipmentId,
                ObservedCycleTime = TimeSpan.FromSeconds(observedRatio * 60),
                IdealCycleTime = TimeSpan.FromSeconds(60),
                ObservedAtUtc = timestamp,
            };

            var cycleAnomaly = _detector.DetectCycleTimeAnomaly(cycleReading);
            if (cycleAnomaly is not null)
            {
                anomalies.Add(Map(cycleAnomaly, timestamp.Value, "CycleTime", cycleReading.CycleTimeRatio));
            }

            var scrapUnits = (int)Math.Round(rng.NextDouble() * 12);
            var productionReading = new ProductionCountReading
            {
                MachineId = equipmentId,
                WindowStartUtc = timestamp,
                WindowEndUtc = timestamp,
                GoodUnitCount = 100,
                ScrapUnitCount = scrapUnits,
            };

            var qualityAnomaly = _detector.DetectQualityDriftAnomaly(productionReading, baselineScrapRate: 0.03);
            if (qualityAnomaly is not null)
            {
                anomalies.Add(Map(qualityAnomaly, timestamp.Value, "ScrapRate", scrapUnits / (double)productionReading.TotalUnitCount));
            }

            if (rng.NextDouble() > 0.7)
            {
                downtimeEvents.Add(new DowntimeEvent
                {
                    MachineId = equipmentId,
                    ReasonCode = "SIGNAL_STOP",
                    StartUtc = timestamp,
                    EndUtc = UtcTimestamp.From(timestamp.Value.AddMinutes(rng.Next(1, 10))),
                    IsPlanned = false,
                });
            }
        }

        var downtimeAnomaly = _detector.DetectDowntimeFrequencyAnomaly(equipmentId, downtimeEvents, TimeSpan.FromHours(WindowPointCount));
        if (downtimeAnomaly is not null)
        {
            anomalies.Add(Map(downtimeAnomaly, asOfUtc, "DowntimeMinutes", downtimeEvents.Count));
        }

        return anomalies;
    }

    private static PredictiveMaintenanceSensorAnomalyDto Map(TelemetryAnomaly anomaly, DateTime timestampUtc, string channelName, double value) => new()
    {
        TimestampUtc = timestampUtc,
        ChannelName = channelName,
        Severity = anomaly.DeviationRatio >= 2.0 ? "Critical" : "Warning",
        Value = Math.Round(value, 4),
        Explanation = anomaly.Message,
    };

    /// <summary>Deterministic per-equipment RNG seed so results are stable across repeated calls for the same equipment.</summary>
    private static Random SeededRandom(Guid equipmentId) => new(equipmentId.GetHashCode());
}
