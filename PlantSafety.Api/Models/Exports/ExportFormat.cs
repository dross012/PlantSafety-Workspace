// ============================================================================
// File: ExportFormat.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Enumeration of supported export file formats for all PlantSafety.Api
//              export endpoints. Bound from query strings as "Csv", "Excel", or "Pdf".
// ============================================================================
//
// Onboarding Notes:
// - Used as a property on ExportRequest. Binds cleanly from a query string
//   ?Format=Csv without a custom model binder because ASP.NET Core enum binding
//   accepts case-insensitive string names.
// - The Application layer is responsible for producing the correct binary format.
//   The Api layer only communicates intent via this enum value.
// - Excel is the default preference for compliance officers (regulatory reports).
//   Csv is preferred for data-pipeline integrations. Pdf is for signed/locked reports.
//
// Rationale:
// - Centralising the format enum prevents each module from defining its own
//   FileFormat, OutputFormat, or ExportType enum independently.
//
// ============================================================================

namespace PlantSafety.Api.Models.Exports;

/// <summary>
/// Supported output file formats for PlantSafety.Api export endpoints.
/// Bound from query string parameter <c>Format</c> on all export controllers.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// Comma-separated values. Best for data-pipeline consumption and bulk
    /// import into third-party systems.
    /// </summary>
    Csv,

    /// <summary>
    /// Microsoft Excel (.xlsx). Best for compliance officers and regulatory
    /// reviewers who need formatted reports with column headers.
    /// </summary>
    Excel,

    /// <summary>
    /// Portable Document Format. Best for signed, locked, audit-trail reports
    /// that must not be editable after generation.
    /// </summary>
    Pdf
}
