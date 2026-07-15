// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to fetch a single Worker WellBeing record by identifier.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Features.WorkerWellBeing.Models.Records;

namespace PlantSafety.Application.Features.WorkerWellBeing.Queries.Records.GetRecord;

/// <summary>Returns the <see cref="WorkerWellBeingDto"/> for a given record, or null.</summary>
public sealed record GetWorkerWellBeingByIdQuery : IQuery<WorkerWellBeingDto?>, ICacheableQuery
{
    /// <inheritdoc/>
    public required string TraceId { get; init; }

    /// <inheritdoc/>
    public required string SpanId { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid RequestingUserId { get; init; }

    /// <inheritdoc/>
    public required string RequestingUserRole { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public CachePreference CachePreference { get; init; } = CachePreference.Preferred;

    /// <inheritdoc/>
    public bool SuppressUnauthorized { get; init; }

    /// <inheritdoc/>
    public bool StrictMode { get; init; }

    /// <summary>Record to retrieve.</summary>
    public required Guid WorkerWellBeingId { get; init; }

    /// <inheritdoc/>
    public string CacheKey => $"worker-wellbeing:{TenantId}:{WorkerWellBeingId}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
