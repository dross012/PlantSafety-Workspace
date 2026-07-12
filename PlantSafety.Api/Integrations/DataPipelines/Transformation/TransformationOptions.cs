// ============================================================================
// File: TransformationOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for the integration data transformation
//              stage. Bound from configuration section
//              "Integrations:DataPipelines:Transformation".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual transformation logic is
//   implemented in the Infrastructure layer; this options class defines the
//   configuration surface the Infrastructure transformation stage binds
//   against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.DataPipelines.Transformation;

/// <summary>Configuration options for the integration data transformation stage.</summary>
public sealed class TransformationOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:DataPipelines:Transformation";

    /// <summary>Whether the transformation stage is enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Identifier of the transformation profile applied to incoming records.</summary>
    public string? TransformationProfile { get; set; }
}
