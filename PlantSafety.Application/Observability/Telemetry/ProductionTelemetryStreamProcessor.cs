// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Application-layer event-stream processor (Phase 5, Section 2.2). Consumes
// batches of real-time production telemetry (cycle times, downtime events, production counts),
// runs the domain TelemetryAnomalyDetector, and produces the anomaly set to be routed into
// Analytics.Dashboard / Analytics.ScoreCards / Dashboards.Widget alerts. Performs no
// infrastructure I/O itself -- callers (MediatR handlers / background jobs) are responsible for
// persisting/publishing the returned anomalies.
// SchemaVersion: 1

using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Observability.Telemetry.Services;
using PlantSafety.Domain.Observability.Telemetry.ValueObjects;

namespace PlantSafety.Application.Observability.Telemetry;

/// <summary>A single machine's telemetry batch for one processing window.</summary>
public sealed record MachineTelemetryBatch
{
    /// <summary>Identity of the machine this batch pertains to.</summary>
    public required Guid MachineId { get; init; }

    /// <summary>Most recent cycle time reading, if any were captured in the window.</summary>
    public CycleTimeReading? LatestCycleTimeReading { get; init; }

    /// <summary>Downtime events recorded within the processing window.</summary>
    public IReadOnlyList<DowntimeEvent> DowntimeEvents { get; init; } = [];

    /// <summary>Aggregated production count reading for the processing window.</summary>
    public ProductionCountReading? ProductionCountReading { get; init; }

    /// <summary>Historical baseline scrap rate (0.0-1.0) used for quality-drift comparison.</summary>
    public double BaselineScrapRate { get; init; }
}

/// <summary>
/// Consumes production telemetry batches and runs the domain anomaly detector, producing the
/// anomaly set for downstream alerting and dashboard aggregation.
/// </summary>
public sealed class ProductionTelemetryStreamProcessor
{
    private static readonly TimeSpan DowntimeAnalysisWindow = TimeSpan.FromHours(8);

    private readonly TelemetryAnomalyDetector _anomalyDetector;

    /// <summary>Creates the processor with the platform-default anomaly detector.</summary>
    public ProductionTelemetryStreamProcessor() : this(new TelemetryAnomalyDetector()) { }

    /// <summary>Creates the processor with an explicit anomaly detector (for testing/composition).</summary>
    public ProductionTelemetryStreamProcessor(TelemetryAnomalyDetector anomalyDetector)
    {
        _anomalyDetector = anomalyDetector;
    }

    /// <summary>
    /// Processes a batch of per-machine telemetry, returning every anomaly detected across
    /// cycle-time degradation, downtime frequency, and quality drift signals.
    /// </summary>
    public IReadOnlyList<TelemetryAnomaly> ProcessBatch(IReadOnlyList<MachineTelemetryBatch> batches)
    {
        ArgumentNullException.ThrowIfNull(batches);

        var anomalies = new List<TelemetryAnomaly>();

        foreach (var batch in batches)
        {
            if (batch.LatestCycleTimeReading is { } cycleTimeReading)
            {
                var cycleAnomaly = _anomalyDetector.DetectCycleTimeAnomaly(cycleTimeReading);
                if (cycleAnomaly is not null)
                    anomalies.Add(cycleAnomaly);
            }

            var downtimeAnomaly = _anomalyDetector.DetectDowntimeFrequencyAnomaly(
                batch.MachineId, batch.DowntimeEvents, DowntimeAnalysisWindow);
            if (downtimeAnomaly is not null)
                anomalies.Add(downtimeAnomaly);

            if (batch.ProductionCountReading is { } productionCountReading)
            {
                var qualityAnomaly = _anomalyDetector.DetectQualityDriftAnomaly(
                    productionCountReading, batch.BaselineScrapRate);
                if (qualityAnomaly is not null)
                    anomalies.Add(qualityAnomaly);
            }
        }

        return anomalies;
    }
}
