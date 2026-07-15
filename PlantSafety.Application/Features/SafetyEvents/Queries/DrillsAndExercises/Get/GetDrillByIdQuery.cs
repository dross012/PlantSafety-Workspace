// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to fetch a single Safety Drill / Exercise by its identifier.
//   Returns the full DrillDto projection.
//   Results are cached per TenantId+DrillId with a 5-minute TTL when CachePreference is Preferred.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Caching;

namespace PlantSafety.Application.Features.SafetyEvents.Queries.DrillsAndExercises.Get;

/// <summary>
/// Returns the full <see cref="Features.SafetyEvents.Models.DrillsAndExercises.DrillDto"/> for a given drill.
/// Returns <c>null</c> when the drill does not exist.
/// </summary>
public sealed record GetDrillByIdQuery : IQuery<Models.DrillsAndExercises.DrillDto?>, ICacheableQuery
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

    /// <summary>Drill to retrieve.</summary>
    public required Guid DrillId { get; init; }

    // --- ICacheableQuery ---

    /// <inheritdoc/>
    public string CacheKey => $"drills:{TenantId}:{DrillId}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
