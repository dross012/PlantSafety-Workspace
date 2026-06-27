// ============================================================================
// File: SortOptions.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Canonical sort field and direction models for all PlantSafety.Api
//              list endpoint query parameters.
// ============================================================================
//
// Onboarding Notes:
// - Add SortField and SortDirection properties to any Get{Sub}Request record that
//   needs server-side sorting. The Application layer query object maps these to
//   the ORDER BY clause of the Dapper stored procedure call.
// - SortDirection uses a string discriminator ("asc"/"desc") rather than an enum
//   so it binds cleanly from query strings without custom model binders.
// - SortOptions.Default() is used in controllers when the caller omits sort params,
//   providing a deterministic default (CreatedAt descending) across all modules.
//
// Rationale:
// - Centralising sort models prevents each of 121 modules from defining its own
//   SortDirection enum and ensures uniform query-string binding behaviour.
//
// Future Extension Seams:
// - Add multi-field sort: IReadOnlyList<SortOptions> for complex sort expressions.
// - Add AllowedFields validation to SortOptions constructor to reject unknown fields.
//
// ============================================================================

namespace PlantSafety.Api.Models;

/// <summary>
/// Sort direction discriminator for list endpoint query parameters.
/// Bound from query strings as "asc" or "desc" (case-insensitive).
/// </summary>
public enum SortDirection
{
    /// <summary>Ascending order (A→Z, oldest→newest).</summary>
    Asc,

    /// <summary>Descending order (Z→A, newest→oldest).</summary>
    Desc
}

/// <summary>
/// Canonical sort specification for PlantSafety.Api list endpoints.
/// Added to <c>Get{Module}Request</c> records that support server-side sorting.
/// </summary>
/// <param name="SortBy">
/// The field name to sort by (e.g. "createdAt", "title", "status").
/// The Application layer validates this against allowed field names.
/// </param>
/// <param name="Direction">Sort direction. Defaults to <see cref="SortDirection.Desc"/>.</param>
public sealed record SortOptions(
    string        SortBy    = "createdAt",
    SortDirection Direction = SortDirection.Desc)
{
    /// <summary>Returns the default sort: <c>createdAt DESC</c>.</summary>
    public static SortOptions Default() => new("createdAt", SortDirection.Desc);

    /// <summary>Returns ascending sort by the specified field.</summary>
    /// <param name="field">Field name to sort by.</param>
    public static SortOptions AscBy(string field) => new(field, SortDirection.Asc);

    /// <summary>Returns descending sort by the specified field.</summary>
    /// <param name="field">Field name to sort by.</param>
    public static SortOptions DescBy(string field) => new(field, SortDirection.Desc);
}
