// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to fetch a single Emergency Response activation by its identifier.
//   Returns the full EmergencyResponseDto projection.
//   Results are cached per TenantId+EmergencyResponseId with a 5-minute TTL when CachePreference is Preferred.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Caching;

namespace PlantSafety.Application.Features.SafetyEvents.Queries.EmergencyManagement.Get;

/// <summary>
/// Returns the full <see cref="Features.SafetyEvents.Models.EmergencyManagement.EmergencyResponseDto"/> for a given response.
/// Returns <c>null</c> when the emergency response does not exist.
/// </summary>
public sealed record GetEmergencyResponseByIdQuery : IQuery<Models.EmergencyManagement.EmergencyResponseDto?>, ICacheableQuery
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

    /// <summary>Emergency response to retrieve.</summary>
    public required Guid EmergencyResponseId { get; init; }

    // --- ICacheableQuery ---

    /// <inheritdoc/>
    public string CacheKey => $"emergency-responses:{TenantId}:{EmergencyResponseId}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
