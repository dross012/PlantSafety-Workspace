// ============================================================================
// File: IngestionOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for the integration data ingestion stage.
//              Bound from configuration section "Integrations:DataPipelines:Ingestion".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual ingestion logic is implemented
//   in the Infrastructure layer; this options class defines the configuration
//   surface the Infrastructure ingestion stage binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.DataPipelines.Ingestion;

/// <summary>Configuration options for the integration data ingestion stage.</summary>
public sealed class IngestionOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:DataPipelines:Ingestion";

    /// <summary>Whether the ingestion stage is enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Maximum number of records accepted per ingestion batch.</summary>
    public int MaxRecordsPerBatch { get; set; } = 1000;
}
