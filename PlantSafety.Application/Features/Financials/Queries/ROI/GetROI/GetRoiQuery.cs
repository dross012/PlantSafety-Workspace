// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to fetch the computed Safety ROI for a tenant/site and fiscal year.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Features.Financials.Models.ROI;

namespace PlantSafety.Application.Features.Financials.Queries.ROI.GetROI;

/// <summary>Returns the <see cref="SafetyROIDto"/> for a given tenant/site/fiscal year, or null.</summary>
public sealed record GetRoiQuery : IQuery<SafetyROIDto?>, ICacheableQuery
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

    /// <summary>Site to scope the calculation to. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Fiscal year to retrieve.</summary>
    public required int FiscalYear { get; init; }

    /// <inheritdoc/>
    public string CacheKey => $"roi:{TenantId}:{SiteId}:{FiscalYear}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(10);
}
