// ============================================================================
// File: IntegrationPipelineOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for integration background pipeline
//              execution. Bound from configuration section
//              "Integrations:BackgroundServices:Pipelines".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual pipeline execution logic is
//   implemented in the Infrastructure layer; this options class defines the
//   configuration surface the Infrastructure pipeline host binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.BackgroundServices.Pipelines;

/// <summary>Configuration options for integration background pipeline execution.</summary>
public sealed class IntegrationPipelineOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:BackgroundServices:Pipelines";

    /// <summary>Whether background pipeline execution is enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Number of records processed per pipeline execution batch.</summary>
    public int BatchSize { get; set; } = 100;
}
