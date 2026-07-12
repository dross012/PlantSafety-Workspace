// ============================================================================
// File: OracleAdapterOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for the Oracle ERP integration adapter.
//              Bound from configuration section "Integrations:Adapters:Oracle".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual adapter connection/mapping logic
//   is implemented in the Infrastructure layer; this options class defines
//   the configuration surface the Infrastructure adapter binds against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Adapters.Oracle;

/// <summary>Configuration options for the Oracle ERP integration adapter.</summary>
public sealed class OracleAdapterOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:Adapters:Oracle";

    /// <summary>Whether the Oracle adapter is enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Base connection endpoint for the Oracle integration.</summary>
    public string? Endpoint { get; set; }

    /// <summary>Timeout in seconds for outbound calls to Oracle.</summary>
    public int TimeoutSeconds { get; set; } = 30;
}
