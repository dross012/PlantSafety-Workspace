// ============================================================================
//  File:           PagedResult.cs
//  Description:    Generic paged result wrapper for API responses.
// ============================================================================

namespace PlantSafety.SharedKernel.Primitives.Results;

public sealed class PagedResult<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
}
