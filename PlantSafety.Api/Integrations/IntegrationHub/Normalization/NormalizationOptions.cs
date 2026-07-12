// ============================================================================
// File: NormalizationOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for integration hub data normalization.
//              Bound from configuration section
//              "Integrations:IntegrationHub:Normalization".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual normalization logic is
//   implemented in the Infrastructure layer; this options class defines the
//   configuration surface the Infrastructure normalization stage binds
//   against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.IntegrationHub.Normalization;

/// <summary>Configuration options for integration hub data normalization.</summary>
public sealed class NormalizationOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:IntegrationHub:Normalization";

    /// <summary>Whether data normalization is enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Default schema version applied when normalizing incoming records.</summary>
    public string? DefaultSchemaVersion { get; set; }
}
