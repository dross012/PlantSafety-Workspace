// ============================================================================
// File: CsvGeneratorOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for the CSV file generator used by
//              integration export features. Bound from configuration section
//              "Integrations:Generators:Csv".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual CSV generation logic is
//   implemented in the Infrastructure layer; this options class defines the
//   configuration surface the Infrastructure generator binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Generators.Csv;

/// <summary>Configuration options for the CSV file generator used by integration export features.</summary>
public sealed class CsvGeneratorOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:Generators:Csv";

    /// <summary>Field delimiter used when generating CSV output.</summary>
    public string Delimiter { get; set; } = ",";

    /// <summary>Whether generated CSV output includes a header row.</summary>
    public bool IncludeHeaders { get; set; } = true;
}
