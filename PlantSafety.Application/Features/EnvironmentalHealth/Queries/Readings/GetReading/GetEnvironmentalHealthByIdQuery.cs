// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to fetch a single Environmental Health reading by identifier.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Features.EnvironmentalHealth.Models.Readings;

namespace PlantSafety.Application.Features.EnvironmentalHealth.Queries.Readings.GetReading;

/// <summary>Returns the <see cref="EnvironmentalHealthDto"/> for a given reading, or null.</summary>
public sealed record GetEnvironmentalHealthByIdQuery : IQuery<EnvironmentalHealthDto?>, ICacheableQuery
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

    /// <summary>Reading to retrieve.</summary>
    public required Guid EnvironmentalHealthId { get; init; }

    /// <inheritdoc/>
    public string CacheKey => $"environmental-health:{TenantId}:{EnvironmentalHealthId}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
