// ============================================================================
// File: PdfGeneratorOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for the PDF file generator used by
//              integration export features. Bound from configuration section
//              "Integrations:Generators:Pdf".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual PDF generation logic is
//   implemented in the Infrastructure layer; this options class defines the
//   configuration surface the Infrastructure generator binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Generators.Pdf;

/// <summary>Configuration options for the PDF file generator used by integration export features.</summary>
public sealed class PdfGeneratorOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:Generators:Pdf";

    /// <summary>Page size used when generating PDF output.</summary>
    public string PageSize { get; set; } = "A4";

    /// <summary>Whether generated PDF output includes a header/footer.</summary>
    public bool IncludeHeaderFooter { get; set; } = true;
}
