// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Contracts
//  Layer:          SharedKernel
//  File:           PagedResult.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Canonical paged query result contract. Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Contracts.Pagination;

/// <summary>
/// Canonical paged query result wrapper.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed record PagedResult<T>
{
    /// <summary>Items for the current page.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>Total number of matching items across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>Current 1-based page number.</summary>
    public int Page { get; init; }

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; init; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>Whether a previous page exists.</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Whether a next page exists.</summary>
    public bool HasNextPage => Page < TotalPages;

    public static PagedResult<T> Empty(int page = 1, int pageSize = 25)
        => new() { Items = [], TotalCount = 0, Page = page, PageSize = pageSize };

    public static PagedResult<T> Of(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
        => new() { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
}
