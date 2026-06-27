// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Query to list Safety Budgets for a tenant and fiscal year.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.Financials.Models.Budgets;

namespace PlantSafety.Application.Features.Financials.Queries.Budgets.ListBudgets;

/// <summary>Returns paginated <see cref="SafetyBudgetDto"/> records.</summary>
public sealed record ListBudgetsQuery : IQuery<PaginatedResult<SafetyBudgetDto>>
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
    public CachePreference CachePreference { get; init; } = CachePreference.None;

    /// <inheritdoc/>
    public bool SuppressUnauthorized { get; init; }

    /// <inheritdoc/>
    public bool StrictMode { get; init; }

    /// <summary>1-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Optional fiscal year filter.</summary>
    public int? FiscalYear { get; init; }

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional category filter.</summary>
    public string? Category { get; init; }

    /// <summary>Optional status filter.</summary>
    public string? Status { get; init; }
}
