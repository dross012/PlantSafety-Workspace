// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Query to fetch a single Safety Budget by identifier.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Features.Financials.Models.Budgets;

namespace PlantSafety.Application.Features.Financials.Queries.Budgets.GetBudget;

/// <summary>Returns the <see cref="SafetyBudgetDto"/> for a given budget, or null.</summary>
public sealed record GetBudgetByIdQuery : IQuery<SafetyBudgetDto?>, ICacheableQuery
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

    /// <summary>Budget to retrieve.</summary>
    public required Guid BudgetId { get; init; }

    /// <inheritdoc/>
    public string CacheKey => $"budgets:{TenantId}:{BudgetId}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(10);
}
