// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to list Worker WellBeing records for a tenant.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.WorkerWellBeing.Models.Records;

namespace PlantSafety.Application.Features.WorkerWellBeing.Queries.Records.ListRecords;

/// <summary>Returns paginated <see cref="WorkerWellBeingDto"/> records.</summary>
public sealed record ListWorkerWellBeingQuery : IQuery<PaginatedResult<WorkerWellBeingDto>>
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
    public CachePreference CachePreference { get; init; } = CachePreference.None;

    /// <inheritdoc/>
    public bool SuppressUnauthorized { get; init; }

    /// <inheritdoc/>
    public bool StrictMode { get; init; }

    /// <summary>1-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Sort field (RecordedAtUtc, WellBeingRiskLevel, Status).</summary>
    public string OrderBy { get; init; } = "RecordedAtUtc";

    /// <summary>Sort direction.</summary>
    public string SortDirection { get; init; } = "DESC";

    /// <summary>Optional worker filter.</summary>
    public Guid? WorkerId { get; init; }

    /// <summary>Optional risk level filter.</summary>
    public string? WellBeingRiskLevel { get; init; }

    /// <summary>Optional status filter.</summary>
    public string? Status { get; init; }
}
