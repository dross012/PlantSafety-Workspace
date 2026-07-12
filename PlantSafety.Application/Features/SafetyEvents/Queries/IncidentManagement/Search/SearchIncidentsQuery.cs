// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Full-text search query across Incident records.
//   Searches title, description, location fields using stored-proc full-text search.
//   Returns PaginatedResult<IncidentSummaryDto>.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Search;

/// <summary>
/// Full-text search across Incidents within the caller's tenant.
/// The <see cref="SearchTerm"/> is passed to the stored procedure; SQL injection prevention
/// is the responsibility of the Infrastructure layer parameterisation.
/// </summary>
public sealed record SearchIncidentsQuery : IQuery<PaginatedResult<Models.IncidentManagement.IncidentSummaryDto>>
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
    public CachePreference CachePreference { get; init; } = CachePreference.None;

    /// <inheritdoc/>
    public bool SuppressUnauthorized { get; init; }

    /// <inheritdoc/>
    public bool StrictMode { get; init; }

    // --- Search ---

    /// <summary>Text to search. Min 2, max 200 characters after normalization.</summary>
    public required string SearchTerm { get; init; }

    // --- Pagination ---

    /// <summary>1-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page.</summary>
    public int PageSize { get; init; } = 25;
}
