// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           ProductionRunKpiInputDto.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Raw KPI/telemetry inputs read from persisted ProductionRun data, shaped so the
//                  Application layer can feed the real Phase 5
//                  Analytics.Dashboard.Kpi.ProductionKpiEngine (OEE/MTBF/MTTR/Throughput/Yield/
//                  ScrapRate) and Observability.Telemetry.TelemetryAnomalyDetector (bottleneck
//                  detection) rather than a second, duplicated KPI/anomaly computation. One instance
//                  represents a single run's accumulated totals plus its recorded downtime events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operational.ProductionTracking.Models;

/// <summary>A single recorded downtime interval for a ProductionRun's machine.</summary>
public sealed record RunDowntimeEventRow
{
    /// <summary>Machine-readable downtime reason code.</summary>
    public required string ReasonCode { get; init; }

    /// <summary>UTC instant the downtime began.</summary>
    public required DateTime StartUtc { get; init; }

    /// <summary>UTC instant the downtime ended, or null when still ongoing.</summary>
    public DateTime? EndUtc { get; init; }

    /// <summary>Whether the downtime was pre-scheduled.</summary>
    public required bool IsPlanned { get; init; }
}

/// <summary>Raw KPI/telemetry inputs for a single ProductionRun, sourced from persisted run data.</summary>
public sealed record ProductionRunKpiInputDto
{
    /// <summary>Run identifier.</summary>
    public required Guid RunId { get; init; }

    /// <summary>Run code, for display.</summary>
    public required string RunCode { get; init; }

    /// <summary>Machine identifier driving this run's telemetry.</summary>
    public required Guid MachineId { get; init; }

    /// <summary>UTC instant the run started.</summary>
    public required DateTime StartedAtUtc { get; init; }

    /// <summary>UTC instant the run ended, or null while still active (elapsed computed against "now").</summary>
    public DateTime? EndedAtUtc { get; init; }

    /// <summary>Cumulative good units produced.</summary>
    public required int GoodUnitCount { get; init; }

    /// <summary>Cumulative scrapped units.</summary>
    public required int ScrapUnitCount { get; init; }

    /// <summary>Rated ideal cycle time (seconds).</summary>
    public required double IdealCycleTimeSeconds { get; init; }

    /// <summary>Cumulative downtime minutes recorded against this run.</summary>
    public required double DowntimeMinutesTotal { get; init; }

    /// <summary>Recorded downtime events for this run (feeds TelemetryAnomalyDetector.DetectDowntimeFrequencyAnomaly).</summary>
    public IReadOnlyList<RunDowntimeEventRow> DowntimeEvents { get; init; } = [];
}
