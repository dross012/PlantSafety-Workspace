// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Query to fetch a single Near Miss by its identifier.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Features.SafetyEvents.Models.NearMisses;

namespace PlantSafety.Application.Features.SafetyEvents.Queries.NearMisses.Get;

/// <summary>
/// Returns the full <see cref="NearMissDto"/> for a given near miss, or <c>null</c> if not found.
/// </summary>
public sealed record GetNearMissByIdQuery : IQuery<NearMissDto?>, ICacheableQuery
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

    /// <summary>Near miss to retrieve.</summary>
    public required Guid NearMissId { get; init; }

    /// <inheritdoc/>
    public string CacheKey => $"near-misses:{TenantId}:{NearMissId}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
