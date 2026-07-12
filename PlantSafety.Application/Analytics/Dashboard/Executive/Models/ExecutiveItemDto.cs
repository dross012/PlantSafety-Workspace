// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single executive dashboard summary metric.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Dashboard.Executive.Models;

/// <summary>A single executive-level summary metric shown on the Executive dashboard.</summary>
public sealed record ExecutiveItemDto
{
    /// <summary>Summary metric identifier.</summary>
    public required Guid SummaryId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the executive metric (e.g., TRIR, Open Corrective Actions).</summary>
    public required string MetricName { get; init; }

    /// <summary>Current value of the metric.</summary>
    public required decimal MetricValue { get; init; }

    /// <summary>Target value for the metric, if defined.</summary>
    public decimal? TargetValue { get; init; }

    /// <summary>Current status of the metric relative to target (e.g., OnTrack, AtRisk, Breached).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this snapshot was captured.</summary>
    public required DateTime SnapshotAtUtc { get; init; }
}
