// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Pagination helper extensions for query request objects.
//   §1.5 (Deterministic Pagination Rule): enforces page number clamp, page size clamp, and
//   default field/direction values for all paginated queries.
// SchemaVersion: 1
// Additive-only evolution: new methods may be added; no existing normalization behavior may change.

using PlantSafety.Application.Common.Constants;

namespace PlantSafety.Application.Common.Extensions;

/// <summary>
/// Extension methods that enforce deterministic pagination constraints on query request objects.
/// Always returns safe, clamped values — never raw caller values that could cause SQL injection or runaway queries.
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Returns a safe, clamped page number. Minimum: 1.
    /// </summary>
    /// <param name="pageNumber">The caller-supplied page number.</param>
    /// <returns>A safe page number ≥ 1.</returns>
    public static int ClampPageNumber(this int pageNumber) =>
        Math.Max(PaginationConstants.MinPageSize, pageNumber);

    /// <summary>
    /// Returns a safe, clamped page size within [<see cref="PaginationConstants.MinPageSize"/>, <see cref="PaginationConstants.MaxPageSize"/>].
    /// </summary>
    /// <param name="pageSize">The caller-supplied page size.</param>
    /// <returns>A safe page size within the permitted range.</returns>
    public static int ClampPageSize(this int pageSize) =>
        Math.Clamp(pageSize, PaginationConstants.MinPageSize, PaginationConstants.MaxPageSize);

    /// <summary>
    /// Returns a safe ORDER BY field name. Returns <see cref="PaginationConstants.DefaultOrderByField"/> when null or whitespace.
    /// Never passes caller-controlled SQL fragments; callers must use whitelist validation.
    /// </summary>
    /// <param name="orderBy">The caller-supplied order-by field name.</param>
    /// <returns>A non-null, trimmed order-by field name.</returns>
    public static string SafeOrderBy(this string? orderBy) =>
        string.IsNullOrWhiteSpace(orderBy) ? PaginationConstants.DefaultOrderByField : orderBy.Trim();

    /// <summary>
    /// Returns a safe sort direction. Returns <see cref="PaginationConstants.DefaultSortDirection"/> when null or invalid.
    /// Only "ASC" and "DESC" are valid; all others are replaced with the default.
    /// </summary>
    /// <param name="direction">The caller-supplied sort direction.</param>
    /// <returns>Either "ASC" or "DESC".</returns>
    public static string SafeSortDirection(this string? direction)
    {
        var upper = direction?.Trim().ToUpperInvariant();
        return upper is "ASC" or "DESC" ? upper : PaginationConstants.DefaultSortDirection;
    }
}
