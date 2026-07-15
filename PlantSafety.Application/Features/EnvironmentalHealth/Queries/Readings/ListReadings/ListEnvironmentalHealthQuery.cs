// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to list Environmental Health readings for a tenant within a date window.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.EnvironmentalHealth.Models.Readings;

namespace PlantSafety.Application.Features.EnvironmentalHealth.Queries.Readings.ListReadings;

/// <summary>Returns paginated <see cref="EnvironmentalHealthDto"/> records.</summary>
public sealed record ListEnvironmentalHealthQuery : IQuery<PaginatedResult<EnvironmentalHealthDto>>
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

    /// <summary>Sort field (RecordedAtUtc, MetricType, Status).</summary>
    public string OrderBy { get; init; } = "RecordedAtUtc";

    /// <summary>Sort direction.</summary>
    public string SortDirection { get; init; } = "DESC";

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional metric type filter.</summary>
    public string? MetricType { get; init; }

    /// <summary>Optional status filter.</summary>
    public string? Status { get; init; }

    /// <summary>Inclusive start of the query window (UTC).</summary>
    public DateTime? From { get; init; }

    /// <summary>Inclusive end of the query window (UTC).</summary>
    public DateTime? To { get; init; }
}
