// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Query to retrieve the Safety Dashboard KPI snapshot.
//   Results cached per TenantId+SiteId with a configurable TTL (default: 15 minutes).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Features.Analytics.Models.Dashboard;

namespace PlantSafety.Application.Features.Analytics.Queries.Dashboard;

/// <summary>
/// Returns the <see cref="SafetyDashboardDto"/> KPI snapshot for the given tenant and optional site.
/// Caller must hold <c>Analytics.Dashboard.View</c>.
/// </summary>
public sealed record GetSafetyDashboardQuery : IQuery<SafetyDashboardDto>, ICacheableQuery
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

    /// <summary>Optional site filter. Null returns tenant-wide roll-up.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the reporting window (defaults to 30 days ago).</summary>
    public DateTimeOffset? WindowFromUtc { get; init; }

    /// <summary>Inclusive end of the reporting window (defaults to now).</summary>
    public DateTimeOffset? WindowToUtc { get; init; }

    /// <inheritdoc/>
    public string CacheKey => $"dashboard:{TenantId}:{SiteId?.ToString() ?? "all"}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(15);
}
