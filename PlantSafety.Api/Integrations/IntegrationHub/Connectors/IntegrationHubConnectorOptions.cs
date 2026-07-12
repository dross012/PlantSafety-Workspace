// ============================================================================
// File: IntegrationHubConnectorOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for integration hub connector
//              infrastructure. Bound from configuration section
//              "Integrations:IntegrationHub:Connectors".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual connector orchestration logic is
//   implemented in the Infrastructure layer; this options class defines the
//   configuration surface the Infrastructure connector host binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.IntegrationHub.Connectors;

/// <summary>Configuration options for integration hub connector infrastructure.</summary>
public sealed class IntegrationHubConnectorOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:IntegrationHub:Connectors";

    /// <summary>Whether integration hub connectors are enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Maximum number of connections a connector may hold open concurrently.</summary>
    public int MaxConcurrentConnections { get; set; } = 10;
}
