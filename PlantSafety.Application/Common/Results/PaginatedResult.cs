// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical paginated result envelope for all list/search query handlers.
//   Required fields mandated by §XVIII (Canonical Pagination Envelope).
//   All paginated queries MUST use this type; no ad-hoc paging DTOs are permitted.
//   PageSize has an enforced maximum; all queries require explicit ORDER BY in SQL.
// SchemaVersion: 1
// Additive-only evolution: new fields may be added with defaults; no existing fields may be removed.

using PlantSafety.Application.Common.Constants;

namespace PlantSafety.Application.Common.Results;

/// <summary>
/// Canonical paginated result envelope carrying deterministic paging metadata and the item set.
/// Returned by all paginated query handlers. Guarantees stable ordering and deterministic TotalCount.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
public sealed class PaginatedResult<T>
{
    /// <summary>Initializes a new <see cref="PaginatedResult{T}"/> with the given items and paging metadata.</summary>
    /// <param name="items">The items for the current page.</param>
    /// <param name="totalCount">The total number of records matching the query (before paging).</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    public PaginatedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = Math.Min(pageSize, PaginationConstants.MaxPageSize);
        TotalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)PageSize);
        StartIndex = (pageNumber - 1) * PageSize + 1;
        EndIndex = Math.Min(pageNumber * PageSize, totalCount);
        HasNextPage = pageNumber < TotalPages;
        HasPreviousPage = pageNumber > 1;
    }

    /// <summary>Gets the items in the current page.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>Gets the total number of records matching the query (before paging).</summary>
    public int TotalCount { get; }

    /// <summary>Gets the current page number (1-based).</summary>
    public int PageNumber { get; }

    /// <summary>Gets the effective page size (capped at <see cref="PaginationConstants.MaxPageSize"/>).</summary>
    public int PageSize { get; }

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages { get; }

    /// <summary>Gets the 1-based index of the first item on this page.</summary>
    public int StartIndex { get; }

    /// <summary>Gets the 1-based index of the last item on this page.</summary>
    public int EndIndex { get; }

    /// <summary>Gets a value indicating whether a next page exists.</summary>
    public bool HasNextPage { get; }

    /// <summary>Gets a value indicating whether a previous page exists.</summary>
    public bool HasPreviousPage { get; }

    /// <summary>Creates an empty paginated result for the given page parameters.</summary>
    /// <param name="pageNumber">The requested page number.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <returns>An empty <see cref="PaginatedResult{T}"/>.</returns>
    public static PaginatedResult<T> Empty(int pageNumber = 1, int pageSize = PaginationConstants.DefaultPageSize) =>
        new([], 0, pageNumber, pageSize);

    /// <summary>Projects the items to a new type while preserving paging metadata.</summary>
    /// <typeparam name="TOut">The output item type.</typeparam>
    /// <param name="map">The mapping function applied to each item.</param>
    /// <returns>A new <see cref="PaginatedResult{TOut}"/> with the mapped items.</returns>
    public PaginatedResult<TOut> Map<TOut>(Func<T, TOut> map) =>
        new(Items.Select(map).ToList().AsReadOnly(), TotalCount, PageNumber, PageSize);
}
