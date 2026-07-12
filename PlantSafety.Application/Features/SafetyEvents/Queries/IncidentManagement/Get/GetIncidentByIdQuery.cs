// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Query to fetch a single Incident by its identifier.
//   Returns the full IncidentDto projection.
//   Results are cached per TenantId+IncidentId with a 5-minute TTL when CachePreference is Preferred.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Caching;

namespace PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Get;

/// <summary>
/// Returns the full <see cref="Features.SafetyEvents.Models.IncidentManagement.IncidentDto"/> for a given incident.
/// Returns <c>null</c> when the incident does not exist or is soft-deleted.
/// </summary>
public sealed record GetIncidentByIdQuery : IQuery<Models.IncidentManagement.IncidentDto?>, ICacheableQuery
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

    /// <summary>Incident to retrieve.</summary>
    public required Guid IncidentId { get; init; }

    // --- ICacheableQuery ---

    /// <inheritdoc/>
    public string CacheKey => $"incidents:{TenantId}:{IncidentId}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
