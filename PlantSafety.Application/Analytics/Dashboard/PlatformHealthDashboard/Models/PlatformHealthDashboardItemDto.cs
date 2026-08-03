// AUTHOR: PlantSafety Platform Team (Claude Code performance-optimization-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Read-model item DTO for a single PlatformHealth BI dashboard KPI row,
//                    sourced from [PlatformHealth].[RequestMetricEvent], aggregated per
//                    business hour. Handbook Phase 6 §5.3 Metrics & Dashboards.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Dashboard.PlatformHealthDashboard.Models;

/// <summary>A single PlatformHealth dashboard KPI row, aggregated per business hour.</summary>
public sealed record PlatformHealthDashboardItemDto
{
    /// <summary>Business hour (UTC, truncated to the hour) this KPI row summarizes.</summary>
    public required DateTime BusinessHour { get; init; }

    /// <summary>Total HTTP requests observed in this hour.</summary>
    public required int TotalRequests { get; init; }

    /// <summary>Count of requests that responded with HTTP 5xx.</summary>
    public required int ErrorCount { get; init; }

    /// <summary>Error rate for this hour, 0.0-1.0.</summary>
    public required double ErrorRate { get; init; }

    /// <summary>P50 (median) request latency in milliseconds.</summary>
    public double? P50LatencyMs { get; init; }

    /// <summary>P95 request latency in milliseconds.</summary>
    public double? P95LatencyMs { get; init; }

    /// <summary>P99 request latency in milliseconds.</summary>
    public double? P99LatencyMs { get; init; }
}
