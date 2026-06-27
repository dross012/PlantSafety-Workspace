// ============================================================================
// File: ExportRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Canonical query-string request model for all PlantSafety.Api
//              export endpoints. Bound via [FromQuery] on GET export actions.
// ============================================================================
//
// Onboarding Notes:
// - Use this class (not a record) for all export controller [FromQuery] parameters.
//   ASP.NET Core's query-string model binder requires a settable property class;
//   records with required init properties do not bind from query strings.
// - MaxRecords is capped at 50,000 by ExportValidationFilter regardless of what
//   the caller supplies. This prevents runaway export queries from overwhelming
//   the reporting database.
// - From and To default to null; the Application layer interprets null From as
//   "beginning of time" and null To as UtcNow.
// - Status is an optional free-text filter that the Application layer maps to
//   the module-specific status enumeration (e.g. "Open", "Closed", "Pending").
//
// Rationale:
// - A single shared export request type prevents each of the 6 export controllers
//   from defining its own near-identical request class.
//
// Future Extension Seams:
// - Add string[]? Columns for caller-specified column subset selection.
// - Add string? SortBy / SortDirection for deterministic ordering in exports.
//
// ============================================================================

namespace PlantSafety.Api.Models.Exports;

/// <summary>
/// Query-string request model for all PlantSafety.Api module export endpoints.
/// Bound via <c>[FromQuery]</c> on <c>GET .../export</c> actions.
/// </summary>
public sealed class ExportRequest
{
    /// <summary>Gets or sets the tenant identifier for which data will be exported.</summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the optional site identifier.
    /// When <c>null</c>, data for all sites within the tenant is exported.
    /// </summary>
    public Guid? SiteId { get; set; }

    /// <summary>
    /// Gets or sets the output file format.
    /// Defaults to <see cref="ExportFormat.Csv"/>.
    /// </summary>
    public ExportFormat Format { get; set; } = ExportFormat.Csv;

    /// <summary>
    /// Gets or sets the inclusive start of the date range filter.
    /// When <c>null</c>, the Application layer uses the earliest available record date.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Gets or sets the inclusive end of the date range filter.
    /// When <c>null</c>, the Application layer uses <see cref="DateTime.UtcNow"/>.
    /// </summary>
    public DateTime? To { get; set; }

    /// <summary>
    /// Gets or sets an optional status filter string (e.g. <c>"Open"</c>, <c>"Closed"</c>).
    /// When <c>null</c>, all statuses are included.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of records to include in the export.
    /// Defaults to 10,000. Capped at 50,000 by <see cref="Filters.ExportValidationFilter"/>.
    /// </summary>
    public int MaxRecords { get; set; } = 10_000;
}
