// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Query to fetch a single Safety Observation by identifier.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Features.SafetyEvents.Models.Observation;

namespace PlantSafety.Application.Features.SafetyEvents.Queries.Observation.Get;

/// <summary>Returns the <see cref="ObservationDto"/> for a given observation, or null.</summary>
public sealed record GetObservationByIdQuery : IQuery<ObservationDto?>, ICacheableQuery
{
    /// <inheritdoc/>
    public required Guid TraceId { get; init; }

    /// <inheritdoc/>
    public required Guid SpanId { get; init; }

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

    /// <summary>Observation to retrieve.</summary>
    public required Guid ObservationId { get; init; }

    /// <inheritdoc/>
    public string CacheKey => $"observations:{TenantId}:{ObservationId}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
