// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Client-side paginated result envelope matching the server-side
//   PaginatedResult<T> canonical shape defined in the Enterprise Architectural
//   Handbook §PART XVIII. All fields are required; no optional fields without defaults.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.5, §XVIII
// ============================================================

using System.Collections.Generic;

namespace PlantSafety.UI.Models.Common;

/// <summary>
/// Client-side representation of the server-side PaginatedResult envelope.
/// All list API responses MUST be deserialized into this type.
/// </summary>
/// <typeparam name="T">The item type in the paginated collection.</typeparam>
public sealed class PaginatedResponseDto<T>
{
    /// <summary>Gets the one-based page number of the current result set.</summary>
    public int PageNumber { get; init; }

    /// <summary>Gets the maximum number of items per page.</summary>
    public int PageSize { get; init; }

    /// <summary>Gets the total count of matching items across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>Gets the total number of pages given the current page size.</summary>
    public int TotalPages { get; init; }

    /// <summary>Gets the zero-based index of the first item on this page.</summary>
    public int StartIndex { get; init; }

    /// <summary>Gets the zero-based index of the last item on this page.</summary>
    public int EndIndex { get; init; }

    /// <summary>Gets a value indicating whether there is a next page.</summary>
    public bool HasNextPage { get; init; }

    /// <summary>Gets a value indicating whether there is a previous page.</summary>
    public bool HasPreviousPage { get; init; }

    /// <summary>Gets the items on the current page. Never null; empty when no results.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];
}
