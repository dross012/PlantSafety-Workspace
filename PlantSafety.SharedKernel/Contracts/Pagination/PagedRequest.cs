// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Contracts
//  Layer:          SharedKernel
//  File:           PagedRequest.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Canonical paged query request contract. Enforces minimum page
//                  size of 1 and maximum of 500. Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Contracts.Pagination;

/// <summary>
/// Canonical paged query request.
/// Page numbers are 1-based. Page size is clamped to [1, 500].
/// </summary>
public sealed record PagedRequest
{
    private const int MinPage     = 1;
    private const int MinPageSize = 1;
    private const int MaxPageSize = 500;
    private const int DefaultPageSize = 25;

    /// <summary>1-based page number.</summary>
    public int Page { get; init; } = MinPage;

    /// <summary>Number of items per page (1–500, default 25).</summary>
    public int PageSize { get; init; } = DefaultPageSize;

    /// <summary>Zero-based offset for SQL OFFSET/FETCH.</summary>
    public int Offset => (Math.Max(Page, MinPage) - 1) * EffectivePageSize;

    /// <summary>Clamped page size safe for SQL FETCH NEXT.</summary>
    public int EffectivePageSize => Math.Clamp(PageSize, MinPageSize, MaxPageSize);

    public static PagedRequest Default => new();
    public static PagedRequest Of(int page, int pageSize) => new() { Page = page, PageSize = pageSize };
}
