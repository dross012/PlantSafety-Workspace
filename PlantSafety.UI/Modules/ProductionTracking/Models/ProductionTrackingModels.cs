// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Production Tracking module
//   (Volume T1) — Dashboard, Run Detail, Batch Tracking, Timeline, KPI
//   Dashboard, and Event History pages. Display-only — no throughput/
//   bottleneck/cycle-time calculation logic.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.ProductionTracking.Models;

/// <summary>Throughput Summary card (current/target/efficiency).</summary>
public sealed class ThroughputSummaryDto
{
    /// <summary>Gets the current throughput in units/hour.</summary>
    public double CurrentThroughput { get; init; }

    /// <summary>Gets the target throughput in units/hour.</summary>
    public double TargetThroughput { get; init; }

    /// <summary>Gets the efficiency percentage (actual/target).</summary>
    public double EfficiencyPercent { get; init; }

    /// <summary>Gets the trend direction (Increasing, Decreasing, Stable).</summary>
    public string TrendDirection { get; init; } = string.Empty;
}

/// <summary>Bottleneck Summary card (active bottlenecks + severity).</summary>
public sealed class BottleneckSummaryDto
{
    /// <summary>Gets the count of active bottlenecks.</summary>
    public int ActiveBottlenecks { get; init; }

    /// <summary>Gets the highest current bottleneck severity (High, Medium, Low).</summary>
    public string Severity { get; init; } = string.Empty;
}

/// <summary>Single row in the Active Production Runs table.</summary>
public sealed class ProductionRunRowDto
{
    /// <summary>Gets the run identifier (UUIDv7).</summary>
    public Guid RunId { get; init; }

    /// <summary>Gets the human-readable run code (format PR-###).</summary>
    public string RunCode { get; init; } = string.Empty;

    /// <summary>Gets the production line name.</summary>
    public string Line { get; init; } = string.Empty;

    /// <summary>Gets the run start time (UTC).</summary>
    public DateTimeOffset StartUtc { get; init; }

    /// <summary>Gets the run status (Running, Paused, Completed, Failed, Pending).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the current throughput in units/hour.</summary>
    public double Throughput { get; init; }
}

/// <summary>Run Summary panel header.</summary>
public sealed class RunSummaryDto
{
    /// <summary>Gets the run identifier.</summary>
    public Guid RunId { get; init; }

    /// <summary>Gets the human-readable run code.</summary>
    public string RunCode { get; init; } = string.Empty;

    /// <summary>Gets the run status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the current throughput in units/hour.</summary>
    public double Throughput { get; init; }

    /// <summary>Gets the throughput trend direction.</summary>
    public string TrendDirection { get; init; } = string.Empty;
}

/// <summary>Run Metadata panel (2-column, labels left / values right).</summary>
public sealed class RunMetadataDto
{
    /// <summary>Gets the production line name.</summary>
    public string Line { get; init; } = string.Empty;

    /// <summary>Gets the run start time (UTC).</summary>
    public DateTimeOffset StartUtc { get; init; }

    /// <summary>Gets the operator display name.</summary>
    public string Operator { get; init; } = string.Empty;
}

/// <summary>Single chronological timeline event row.</summary>
public sealed class TimelineEventDto
{
    /// <summary>Gets the event identifier.</summary>
    public Guid EventId { get; init; }

    /// <summary>Gets the event timestamp (UTC).</summary>
    public DateTimeOffset TimestampUtc { get; init; }

    /// <summary>Gets the event type (Start, Step, Pause, Error, Resume, Complete).</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>Gets a description of the event.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the related production run identifier.</summary>
    public Guid RunId { get; init; }
}

/// <summary>Single row in the Batch List table.</summary>
public sealed class BatchRowDto
{
    /// <summary>Gets the batch identifier.</summary>
    public Guid BatchId { get; init; }

    /// <summary>Gets the human-readable batch code (format B-###).</summary>
    public string BatchCode { get; init; } = string.Empty;

    /// <summary>Gets the associated run identifier.</summary>
    public Guid RunId { get; init; }

    /// <summary>Gets the associated run code.</summary>
    public string RunCode { get; init; } = string.Empty;

    /// <summary>Gets the production line name.</summary>
    public string Line { get; init; } = string.Empty;

    /// <summary>Gets the batch status (Active, Completed, Failed, Pending).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the batch start time (UTC).</summary>
    public DateTimeOffset StartUtc { get; init; }
}

/// <summary>Single KPI tile (value + target + status).</summary>
public sealed class KpiTileDto
{
    /// <summary>Gets the KPI metric name (e.g. "Throughput", "OEE", "Scrap Rate").</summary>
    public string MetricName { get; init; } = string.Empty;

    /// <summary>Gets the current KPI value.</summary>
    public double Value { get; init; }

    /// <summary>Gets the KPI target value.</summary>
    public double Target { get; init; }

    /// <summary>Gets the unit of measure (e.g. "units/hr", "%").</summary>
    public string Unit { get; init; } = string.Empty;

    /// <summary>Gets the KPI status relative to target (Met, Warning, Missed).</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Single row in the Production Event History log.</summary>
public sealed class EventRowDto
{
    /// <summary>Gets the event identifier.</summary>
    public Guid EventId { get; init; }

    /// <summary>Gets the event timestamp (UTC).</summary>
    public DateTimeOffset TimestampUtc { get; init; }

    /// <summary>Gets the related run code.</summary>
    public string RunCode { get; init; } = string.Empty;

    /// <summary>Gets the related run identifier, for drilldown.</summary>
    public Guid RunId { get; init; }

    /// <summary>Gets the event type.</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>Gets a description of the event.</summary>
    public string Description { get; init; } = string.Empty;
}
