// ============================================================================
// File: EtlPipelineOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for the integration ETL pipeline stage.
//              Bound from configuration section "Integrations:DataPipelines:ETL".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual ETL extraction/transform/load logic
//   is implemented in the Infrastructure layer; this options class defines the
//   configuration surface the Infrastructure ETL stage binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.DataPipelines.ETL;

/// <summary>Configuration options for the integration ETL pipeline stage.</summary>
public sealed class EtlPipelineOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:DataPipelines:ETL";

    /// <summary>Whether the ETL pipeline stage is enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Number of records processed per ETL batch.</summary>
    public int BatchSize { get; set; } = 500;

    /// <summary>Identifier of the upstream source system feeding this ETL stage.</summary>
    public string? SourceSystem { get; set; }
}
