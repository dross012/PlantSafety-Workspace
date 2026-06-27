// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical sort direction enum for all paginated query objects.
//   Used in IQuery implementations and passed to repository parameter objects.
//   Simple enum (no behavior, no metadata) — Smart Enum not warranted per §2.7.
// SchemaVersion: 1
// Additive-only evolution: new values may be added; existing values must not be renamed or removed.

namespace PlantSafety.Application.Common.Enums;

/// <summary>
/// Specifies the sort direction for a paginated query result set.
/// Maps directly to SQL <c>ORDER BY ... ASC/DESC</c>.
/// </summary>
public enum SortDirection
{
    /// <summary>Sort ascending (smallest to largest, oldest to newest).</summary>
    Ascending = 0,

    /// <summary>Sort descending (largest to smallest, newest to oldest).</summary>
    Descending = 1
}
