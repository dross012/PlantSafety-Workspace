// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical paginated request DTO shared by all list/search API endpoints.
//   §1.5 (Deterministic Pagination Rule): PageNumber and PageSize are always required.
//   §1.4 (Canonical DTO Rule): immutable, normalized, fully documented, no optional fields without defaults.
// SchemaVersion: 1
// Additive-only evolution: new fields may be added with defaults; no existing fields may be removed or renamed.

using PlantSafety.Application.Common.Constants;

namespace PlantSafety.Application.Contracts.Requests;

/// <summary>
/// Canonical base DTO for all paginated list and search requests at the API boundary.
/// Applied to every GET endpoint that returns a list of items.
/// Maps to query objects via explicit factory methods — never AutoMapper.
/// </summary>
public sealed record PaginatedRequestDto
{
    /// <summary>Gets the requested page number (1-based). Defaults to 1.</summary>
    public int PageNumber { get; init; } = PaginationConstants.DefaultPageNumber;

    /// <summary>Gets the requested page size. Defaults to 25; maximum 250.</summary>
    public int PageSize { get; init; } = PaginationConstants.DefaultPageSize;

    /// <summary>Gets the field name to order results by. Defaults to CreatedAtUtc.</summary>
    public string OrderBy { get; init; } = PaginationConstants.DefaultOrderByField;

    /// <summary>Gets the sort direction (ASC or DESC). Defaults to DESC.</summary>
    public string SortDirection { get; init; } = PaginationConstants.DefaultSortDirection;

    /// <summary>Gets the optional free-text search term. Null or whitespace is treated as no filter.</summary>
    public string? SearchTerm { get; init; }
}
