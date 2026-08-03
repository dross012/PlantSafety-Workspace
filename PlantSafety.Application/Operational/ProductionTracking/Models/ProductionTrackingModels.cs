// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           ProductionTrackingModels.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-side DTOs backing the ProductionTracking UI's Dashboard/Run Detail/Batch
//                  Tracking/Timeline/KPI Dashboard/Event History pages (v1/production-tracking/*).
//                  Consolidated into one file per the sibling WorkOrderModels.cs precedent of
//                  grouping closely-related small record types. Field shapes are grounded in
//                  PlantSafety.UI.Modules.ProductionTracking.Models.ProductionTrackingModels.cs (the
//                  pre-existing UI-side DTOs this module now backs for real) -- names/fields match
//                   1:1 so the API boundary mapping is a pure pass-through. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operational.ProductionTracking.Models;

/// <summary>Throughput Summary card (current/target/efficiency).</summary>
public sealed record ThroughputSummaryDto
{
    /// <summary>Current throughput in units/hour, summed across active runs.</summary>
    public required double CurrentThroughput { get; init; }

    /// <summary>Target throughput in units/hour, derived from each active run's rated ideal cycle time.</summary>
    public required double TargetThroughput { get; init; }

    /// <summary>Efficiency percentage (actual/target), 0-100+.</summary>
    public required double EfficiencyPercent { get; init; }

    /// <summary>Trend direction (Increasing, Decreasing, Stable).</summary>
    public required string TrendDirection { get; init; }
}

/// <summary>Bottleneck Summary card (active bottlenecks + severity). Reuse point: computed via the real Observability.Telemetry.TelemetryAnomalyDetector.</summary>
public sealed record BottleneckSummaryDto
{
    /// <summary>Count of active bottleneck anomalies (cycle-time degradation + downtime frequency spikes) across active runs.</summary>
    public required int ActiveBottlenecks { get; init; }

    /// <summary>Highest current bottleneck severity (High, Medium, Low, None).</summary>
    public required string Severity { get; init; }
}

/// <summary>Single row in the Active Production Runs table.</summary>
public sealed record ProductionRunRowDto
{
    /// <summary>Run identifier.</summary>
    public required Guid RunId { get; init; }

    /// <summary>Human-readable run code (format PR-###).</summary>
    public required string RunCode { get; init; }

    /// <summary>Production line name.</summary>
    public required string Line { get; init; }

    /// <summary>Run start time (UTC).</summary>
    public required DateTimeOffset StartUtc { get; init; }

    /// <summary>Run status (Running, Paused, Completed, Failed, Pending).</summary>
    public required string Status { get; init; }

    /// <summary>Current throughput in units/hour.</summary>
    public required double Throughput { get; init; }
}

/// <summary>Run Summary panel header.</summary>
public sealed record RunSummaryDto
{
    /// <summary>Run identifier.</summary>
    public required Guid RunId { get; init; }

    /// <summary>Human-readable run code.</summary>
    public required string RunCode { get; init; }

    /// <summary>Run status.</summary>
    public required string Status { get; init; }

    /// <summary>Current throughput in units/hour.</summary>
    public required double Throughput { get; init; }

    /// <summary>Throughput trend direction.</summary>
    public required string TrendDirection { get; init; }
}

/// <summary>Run Metadata panel.</summary>
public sealed record RunMetadataDto
{
    /// <summary>Production line name.</summary>
    public required string Line { get; init; }

    /// <summary>Run start time (UTC).</summary>
    public required DateTimeOffset StartUtc { get; init; }

    /// <summary>Operator display name.</summary>
    public required string Operator { get; init; }
}

/// <summary>Single chronological timeline event row.</summary>
public sealed record TimelineEventDto
{
    /// <summary>Event identifier.</summary>
    public required Guid EventId { get; init; }

    /// <summary>Event timestamp (UTC).</summary>
    public required DateTimeOffset TimestampUtc { get; init; }

    /// <summary>Event type (Start, Step, Pause, Error, Resume, Complete).</summary>
    public required string EventType { get; init; }

    /// <summary>Description of the event.</summary>
    public required string Description { get; init; }

    /// <summary>Related production run identifier.</summary>
    public required Guid RunId { get; init; }
}

/// <summary>Single row in the Batch List table.</summary>
public sealed record BatchRowDto
{
    /// <summary>Batch identifier.</summary>
    public required Guid BatchId { get; init; }

    /// <summary>Human-readable batch code (format B-###).</summary>
    public required string BatchCode { get; init; }

    /// <summary>Associated run identifier.</summary>
    public required Guid RunId { get; init; }

    /// <summary>Associated run code.</summary>
    public required string RunCode { get; init; }

    /// <summary>Production line name.</summary>
    public required string Line { get; init; }

    /// <summary>Batch status (Active, Completed, Failed, Pending).</summary>
    public required string Status { get; init; }

    /// <summary>Batch start time (UTC).</summary>
    public required DateTimeOffset StartUtc { get; init; }
}

/// <summary>
/// Single KPI tile (value + target + status). Reuse point: Value is computed by the real
/// PlantSafety.Application.Analytics.Dashboard.Kpi.ProductionKpiEngine, not reimplemented here.
/// </summary>
public sealed record KpiTileDto
{
    /// <summary>KPI metric name (Throughput, OEE, MTBF, MTTR, Yield, Scrap Rate).</summary>
    public required string MetricName { get; init; }

    /// <summary>Current KPI value.</summary>
    public required double Value { get; init; }

    /// <summary>KPI target value.</summary>
    public required double Target { get; init; }

    /// <summary>Unit of measure (e.g. "units/hr", "%", "hours").</summary>
    public required string Unit { get; init; }

    /// <summary>KPI status relative to target (Met, Warning, Missed).</summary>
    public required string Status { get; init; }
}

/// <summary>Single row in the Production Event History log.</summary>
public sealed record EventRowDto
{
    /// <summary>Event identifier.</summary>
    public required Guid EventId { get; init; }

    /// <summary>Event timestamp (UTC).</summary>
    public required DateTimeOffset TimestampUtc { get; init; }

    /// <summary>Related run code.</summary>
    public required string RunCode { get; init; }

    /// <summary>Related run identifier, for drilldown.</summary>
    public required Guid RunId { get; init; }

    /// <summary>Event type.</summary>
    public required string EventType { get; init; }

    /// <summary>Description of the event.</summary>
    public required string Description { get; init; }
}
