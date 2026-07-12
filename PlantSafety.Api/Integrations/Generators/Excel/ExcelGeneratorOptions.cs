// ============================================================================
// File: ExcelGeneratorOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for the Excel file generator used by
//              integration export features. Bound from configuration section
//              "Integrations:Generators:Excel".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual Excel generation logic is
//   implemented in the Infrastructure layer; this options class defines the
//   configuration surface the Infrastructure generator binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Generators.Excel;

/// <summary>Configuration options for the Excel file generator used by integration export features.</summary>
public sealed class ExcelGeneratorOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:Generators:Excel";

    /// <summary>Default worksheet name used when generating Excel output.</summary>
    public string SheetName { get; set; } = "Export";

    /// <summary>Whether generated Excel output includes a header row.</summary>
    public bool IncludeHeaders { get; set; } = true;
}
