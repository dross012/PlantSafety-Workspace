// ============================================================================
// File: PagedResult.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Canonical Presentation-layer pagination wrapper used as the Data
//              payload inside ApiResponse<PagedResult<T>> for all list endpoints.
// ============================================================================
//
// Onboarding Notes:
// - All module *PagedResponse types should use PagedResult<TItem> as their Data
//   payload instead of defining their own TotalCount/PageNumber/PageSize fields.
//   This ensures consistent pagination metadata across all 121+ list endpoints.
// - HasNextPage and HasPreviousPage are computed from TotalCount, PageNumber, and
//   PageSize, so callers never need to derive them client-side.
// - The Empty factory method is used in health checks and tests where a populated
//   list is not available.
//
// Rationale:
// - A single canonical paged type prevents per-module pagination shape divergence
//   and gives front-end clients a single contract to parse.
//
// Future Extension Seams:
// - Add ContinuationToken (string?) for cursor-based pagination when offset paging
//   is replaced for high-volume modules (e.g. SafetyEvents, Analytics).
// - Add TotalPages computed property if UI teams request it.
//
// ============================================================================

namespace PlantSafety.Api.Models;

/// <summary>
/// Canonical paginated result wrapper for all PlantSafety.Api list endpoints.
/// Returned as the <c>Data</c> payload inside <see cref="ApiResponse{T}"/>.
/// </summary>
/// <typeparam name="T">The item type in the page.</typeparam>
/// <param name="Items">The current page of items.</param>
/// <param name="TotalCount">Total number of items across all pages.</param>
/// <param name="PageNumber">The current page number (one-based).</param>
/// <param name="PageSize">The number of items per page.</param>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    /// <summary>Gets a value indicating whether a previous page exists.</summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>Gets a value indicating whether a next page exists.</summary>
    public bool HasNextPage => PageNumber * PageSize < TotalCount;

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>Creates an empty <see cref="PagedResult{T}"/> with zero items.</summary>
    /// <returns>An empty paged result for page 1.</returns>
    public static PagedResult<T> Empty() => new([], 0, 1, 25);

    /// <summary>
    /// Creates a <see cref="PagedResult{T}"/> from an already-fetched list where
    /// the caller knows the total count separately (e.g. from a COUNT query).
    /// </summary>
    /// <param name="items">The current page of items.</param>
    /// <param name="totalCount">Total items across all pages.</param>
    /// <param name="pageNumber">Current page number (one-based).</param>
    /// <param name="pageSize">Page size.</param>
    public static PagedResult<T> From(
        IEnumerable<T> items,
        int totalCount,
        int pageNumber,
        int pageSize) =>
        new(items.ToList().AsReadOnly(), totalCount, pageNumber, pageSize);
}
