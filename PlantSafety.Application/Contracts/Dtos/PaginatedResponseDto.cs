// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical paginated response DTO for all list/search API responses.
//   Maps from PaginatedResult<T> at the API boundary; never exposes domain objects.
//   §XVIII (Canonical Pagination Envelope): all required fields present.
// SchemaVersion: 1
// Additive-only evolution: new fields may be added with defaults; no existing fields may be removed.

using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Contracts.Dtos;

/// <summary>
/// Canonical paginated response envelope for all list and search API responses.
/// The API controller maps <see cref="PaginatedResult{T}"/> to this DTO before serialization.
/// </summary>
/// <typeparam name="T">The item DTO type. Must never be a domain object.</typeparam>
public sealed record PaginatedResponseDto<T>
{
    /// <summary>Gets the items in the current page.</summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>Gets the total number of records matching the query (before paging).</summary>
    public required int TotalCount { get; init; }

    /// <summary>Gets the current page number (1-based).</summary>
    public required int PageNumber { get; init; }

    /// <summary>Gets the effective page size.</summary>
    public required int PageSize { get; init; }

    /// <summary>Gets the total number of pages.</summary>
    public required int TotalPages { get; init; }

    /// <summary>Gets the 1-based index of the first item on this page.</summary>
    public required int StartIndex { get; init; }

    /// <summary>Gets the 1-based index of the last item on this page.</summary>
    public required int EndIndex { get; init; }

    /// <summary>Gets a value indicating whether a next page exists.</summary>
    public required bool HasNextPage { get; init; }

    /// <summary>Gets a value indicating whether a previous page exists.</summary>
    public required bool HasPreviousPage { get; init; }

    /// <summary>
    /// Creates a <see cref="PaginatedResponseDto{T}"/> from a <see cref="PaginatedResult{T}"/>.
    /// </summary>
    /// <param name="result">The paginated result from the query handler.</param>
    /// <returns>A new <see cref="PaginatedResponseDto{T}"/>.</returns>
    public static PaginatedResponseDto<T> FromResult(PaginatedResult<T> result) => new()
    {
        Items = result.Items,
        TotalCount = result.TotalCount,
        PageNumber = result.PageNumber,
        PageSize = result.PageSize,
        TotalPages = result.TotalPages,
        StartIndex = result.StartIndex,
        EndIndex = result.EndIndex,
        HasNextPage = result.HasNextPage,
        HasPreviousPage = result.HasPreviousPage
    };
}
