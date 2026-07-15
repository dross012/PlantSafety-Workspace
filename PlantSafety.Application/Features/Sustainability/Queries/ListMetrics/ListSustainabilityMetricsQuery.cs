// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to list sustainability metrics for a tenant within a reporting window.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.Sustainability.Models;

namespace PlantSafety.Application.Features.Sustainability.Queries.ListMetrics;

/// <summary>Returns sustainability metrics for a tenant within the requested reporting window.</summary>
public sealed record ListSustainabilityMetricsQuery : IQuery<IReadOnlyList<SustainabilityMetricDto>>
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

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the reporting window (UTC).</summary>
    public required DateTime From { get; init; }

    /// <summary>Inclusive end of the reporting window (UTC).</summary>
    public required DateTime To { get; init; }
}
