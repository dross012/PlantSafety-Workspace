// ============================================================================
// File: IntegrationHubRoutingOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for integration hub message routing.
//              Bound from configuration section "Integrations:IntegrationHub:Routing".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual routing decision logic is
//   implemented in the Infrastructure layer; this options class defines the
//   configuration surface the Infrastructure routing stage binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.IntegrationHub.Routing;

/// <summary>Configuration options for integration hub message routing.</summary>
public sealed class IntegrationHubRoutingOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:IntegrationHub:Routing";

    /// <summary>Whether integration hub routing is enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Default destination applied when no specific routing rule matches.</summary>
    public string? DefaultDestination { get; set; }
}
