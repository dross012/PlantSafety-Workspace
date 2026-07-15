// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Application-layer repository interface for the EnvironmentalHealth read model.
//   Distinct from PlantSafety.Domain.EnvironmentalHealth.Repositories.IEnvironmentalHealthRepository
//   (the aggregate-oriented domain repository); this interface serves the CQRS read/write
//   slice backing the Features/EnvironmentalHealth Application handlers.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.EnvironmentalHealth.Handlers.Readings;

/// <summary>Repository for EnvironmentalHealth reading read/write operations.</summary>
public interface IEnvironmentalHealthReadingRepository
{
    /// <summary>Returns the read model, or null if not found.</summary>
    Task<EnvironmentalHealthReadModel?> GetByIdAsync(Guid tenantId, Guid environmentalHealthId, CancellationToken cancellationToken);

    /// <summary>Returns paged records matching the given filters.</summary>
    Task<(IReadOnlyList<EnvironmentalHealthReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? metricType,
        string? status,
        DateTime? from,
        DateTime? to,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for EnvironmentalHealth stored-procedure result sets.</summary>
public sealed record EnvironmentalHealthReadModel
{
    /// <summary>Reading identifier.</summary>
    public required Guid EnvironmentalHealthId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier. Null means enterprise-level.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Environmental metric type.</summary>
    public required string MetricType { get; init; }
    /// <summary>Measured value.</summary>
    public required decimal MeasurementValue { get; init; }
    /// <summary>Unit of measure.</summary>
    public required string Unit { get; init; }
    /// <summary>Evaluation threshold.</summary>
    public required decimal Threshold { get; init; }
    /// <summary>Evaluated status.</summary>
    public required string Status { get; init; }
    /// <summary>Recorded timestamp.</summary>
    public required DateTimeOffset RecordedAtUtc { get; init; }
}
