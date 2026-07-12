// ============================================================================
// File: DataPipelineRoutingOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for routing decisions within the
//              integration data pipeline. Bound from configuration section
//              "Integrations:DataPipelines:Routing".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual routing decision logic is
//   implemented in the Infrastructure layer; this options class defines the
//   configuration surface the Infrastructure routing stage binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.DataPipelines.Routing;

/// <summary>Configuration options for routing decisions within the integration data pipeline.</summary>
public sealed class DataPipelineRoutingOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:DataPipelines:Routing";

    /// <summary>Whether data pipeline routing is enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Default route applied when no specific routing rule matches.</summary>
    public string? DefaultRoute { get; set; }
}
