// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO for a single ingested analytics record.
// SchemaVersion: 1

namespace PlantSafety.Application.Integrations.Analytics.Models;

/// <summary>A single analytics record ingested from an external integrated system.</summary>
public sealed record AnalyticsItemDto
{
    /// <summary>Ingested analytics record identifier.</summary>
    public required Guid RecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the external analytics system the record was ingested from.</summary>
    public required string SourceSystem { get; init; }

    /// <summary>Name of the ingested analytics metric.</summary>
    public required string MetricName { get; init; }

    /// <summary>Value of the ingested analytics metric.</summary>
    public required decimal MetricValue { get; init; }

    /// <summary>Current processing status of the ingested record (e.g., Processed/Pending/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the record was ingested.</summary>
    public required DateTime IngestedAtUtc { get; init; }

    /// <summary>Identity of the integration account or user that triggered ingestion.</summary>
    public required Guid IngestedBy { get; init; }
}
