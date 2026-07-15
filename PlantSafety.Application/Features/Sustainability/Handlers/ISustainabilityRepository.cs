// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Repository interface for the sustainability metric read/write model.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Sustainability.Handlers;

/// <summary>Repository for sustainability metric read/write operations.</summary>
public interface ISustainabilityRepository
{
    /// <summary>Records a new sustainability metric observation; stages a SustainabilityMetricRecordedEvent in the outbox.</summary>
    Task CreateAsync(
        Guid metricId,
        Guid tenantId,
        Guid? siteId,
        string metricName,
        string category,
        double value,
        string unit,
        double? target,
        DateTime periodEndUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<SustainabilityMetricReadModel?> GetByIdAsync(Guid tenantId, Guid metricId, CancellationToken cancellationToken);

    /// <summary>Returns metrics for a tenant within the requested reporting window.</summary>
    Task<IReadOnlyList<SustainabilityMetricReadModel>> ListAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for sustainability metric stored-procedure result sets.</summary>
public sealed record SustainabilityMetricReadModel
{
    /// <summary>Metric identifier.</summary>
    public required Guid MetricId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Metric display name.</summary>
    public required string MetricName { get; init; }
    /// <summary>Metric category.</summary>
    public required string Category { get; init; }
    /// <summary>Measured value.</summary>
    public required double Value { get; init; }
    /// <summary>Unit of measure.</summary>
    public required string Unit { get; init; }
    /// <summary>Target value, or null.</summary>
    public double? Target { get; init; }
    /// <summary>UTC end of reporting period.</summary>
    public required DateTime PeriodEndUtc { get; init; }
    /// <summary>Status against target.</summary>
    public required string Status { get; init; }
}
