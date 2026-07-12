// ============================================================================
// File: OutboundWebhookOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for outbound integration webhook
//              dispatch infrastructure. Bound from configuration section
//              "Integrations:Webhooks:OutBound".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual outbound webhook dispatch logic
//   is implemented in the Infrastructure layer; this options class defines
//   the configuration surface the Infrastructure webhook dispatcher binds
//   against.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Webhooks.OutBound;

/// <summary>Configuration options for outbound integration webhook dispatch infrastructure.</summary>
public sealed class OutboundWebhookOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:Webhooks:OutBound";

    /// <summary>Whether outbound webhook dispatch is enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Timeout in seconds for outbound webhook delivery attempts.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Maximum number of delivery retry attempts for a failed webhook.</summary>
    public int MaxRetries { get; set; } = 3;
}
