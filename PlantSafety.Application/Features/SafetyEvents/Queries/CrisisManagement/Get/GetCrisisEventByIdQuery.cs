// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to fetch a single Crisis Management event by its identifier.
//   Returns the full CrisisEventDto projection.
//   Results are cached per TenantId+CrisisEventId with a 5-minute TTL when CachePreference is Preferred.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Caching;

namespace PlantSafety.Application.Features.SafetyEvents.Queries.CrisisManagement.Get;

/// <summary>
/// Returns the full <see cref="Features.SafetyEvents.Models.CrisisManagement.CrisisEventDto"/> for a given crisis event.
/// Returns <c>null</c> when the crisis event does not exist.
/// </summary>
public sealed record GetCrisisEventByIdQuery : IQuery<Models.CrisisManagement.CrisisEventDto?>, ICacheableQuery
{
    // --- Canonical envelope ---

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

    // --- Domain payload ---

    /// <summary>Crisis event to retrieve.</summary>
    public required Guid CrisisEventId { get; init; }

    // --- ICacheableQuery ---

    /// <inheritdoc/>
    public string CacheKey => $"crisis-events:{TenantId}:{CrisisEventId}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
