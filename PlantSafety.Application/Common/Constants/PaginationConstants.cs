// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical pagination constants enforced across all paginated endpoints.
//   §1.5 (Deterministic Pagination Rule): all paginated endpoints must require explicit PageNumber
//   and PageSize, enforce max page size, and guarantee stable ordering.
// SchemaVersion: 1
// Additive-only evolution: values may be updated; no constants may be removed.

namespace PlantSafety.Application.Common.Constants;

/// <summary>
/// Canonical pagination constants for the Application layer.
/// All paginated queries must use these constants — no hardcoded page sizes elsewhere.
/// </summary>
public static class PaginationConstants
{
    /// <summary>The default page number for paginated queries (1-based).</summary>
    public const int DefaultPageNumber = 1;

    /// <summary>The default page size when the caller does not specify one.</summary>
    public const int DefaultPageSize = 25;

    /// <summary>The maximum permitted page size. Requests exceeding this are clamped to this value.</summary>
    public const int MaxPageSize = 250;

    /// <summary>The minimum permitted page size. Requests below this are clamped to this value.</summary>
    public const int MinPageSize = 1;

    /// <summary>The default sort field name when no OrderBy is specified.</summary>
    public const string DefaultOrderByField = "CreatedAtUtc";

    /// <summary>The default sort direction for all paginated queries.</summary>
    public const string DefaultSortDirection = "DESC";
}
