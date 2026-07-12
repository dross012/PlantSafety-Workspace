// ============================================================================
// File: InboundWebhookOptions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-06-13
// Description: Configuration options for inbound integration webhook
//              processing infrastructure. Bound from configuration section
//              "Integrations:Webhooks:Inbound".
// ============================================================================
//
// Onboarding Notes:
// - This is a scaffold placeholder. Actual inbound webhook receipt/validation
//   logic is implemented in the Infrastructure layer; this options class
//   defines the configuration surface the Infrastructure webhook handler
//   binds against. Distinct from the top-level PlantSafety.Api\Webhooks
//   REST receiver, which handles the actual HTTP endpoint.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Webhooks.Inbound;

/// <summary>Configuration options for inbound integration webhook processing infrastructure.</summary>
public sealed class InboundWebhookOptions
{
    /// <summary>Configuration section name this options class binds to.</summary>
    public const string SectionName = "Integrations:Webhooks:Inbound";

    /// <summary>Whether inbound webhook processing is enabled for this tenant/environment.</summary>
    public bool Enabled { get; set; }

    /// <summary>Maximum accepted payload size, in bytes, for an inbound webhook request.</summary>
    public int MaxPayloadSizeBytes { get; set; } = 1048576;
}
