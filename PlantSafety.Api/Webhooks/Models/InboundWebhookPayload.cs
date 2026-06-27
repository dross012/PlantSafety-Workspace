// ============================================================================
// File: InboundWebhookPayload.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Deserialized payload model for inbound webhook POST requests received
//              at the PlantSafety.Api webhook receiver endpoint. Represents the
//              canonical envelope structure that all external systems must conform to.
// ============================================================================
//
// Onboarding Notes:
// - All external systems sending webhooks to PlantSafety must use this envelope.
//   The webhook contract is published in the Architecture Specification
//   (Integration Events section).
// - EventId is a UUID supplied by the sender for idempotency. The Application
//   layer stores processed EventIds to prevent duplicate processing on retry.
// - Data is a JsonElement (raw JSON) so the Application layer can deserialize it
//   to the correct event-specific type based on EventType, without the Api layer
//   needing to know about all event payload shapes.
// - Source identifies the external system (e.g. "SAPPlantMaintenance",
//   "IoTPlatform", "HRSystem"). Used for telemetry attribution.
// - Signature verification (HMAC-SHA256) is performed by WebhookSignatureVerifier
//   BEFORE this payload is deserialized. The receiver controller reads raw body
//   for signature check, then deserializes to this type.
//
// Rationale:
// - A strongly-typed envelope with a raw-JSON Data field balances type safety
//   (the envelope is verified at the Presentation layer) with flexibility
//   (the payload is interpreted by the Application layer).
//
// ============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlantSafety.Api.Webhooks.Models;

/// <summary>
/// Canonical envelope for all inbound webhook payloads received at
/// <c>POST api/v1/webhooks/receive</c>.
/// </summary>
/// <param name="EventId">
/// Unique identifier for this event, supplied by the sending system.
/// Used for idempotency deduplication in the Application layer.
/// </param>
/// <param name="EventType">
/// Discriminator identifying the type of event. Drives Application-layer dispatch.
/// </param>
/// <param name="OccurredAt">UTC timestamp when the event occurred in the source system.</param>
/// <param name="Source">
/// Identifier of the source system (e.g. <c>"IoTPlatform"</c>, <c>"SAPPlantMaintenance"</c>).
/// </param>
/// <param name="TenantId">The PlantSafety tenant this event belongs to.</param>
/// <param name="SiteId">Optional site scope within the tenant.</param>
/// <param name="Data">
/// Event-specific payload as raw JSON. Deserialized by the Application layer
/// command handler based on <see cref="EventType"/>.
/// </param>
public sealed record InboundWebhookPayload(
    [property: JsonPropertyName("eventId")]    string         EventId,
    [property: JsonPropertyName("eventType")]  WebhookEventType EventType,
    [property: JsonPropertyName("occurredAt")] DateTimeOffset OccurredAt,
    [property: JsonPropertyName("source")]     string         Source,
    [property: JsonPropertyName("tenantId")]   Guid           TenantId,
    [property: JsonPropertyName("siteId")]     Guid?          SiteId,
    [property: JsonPropertyName("data")]       JsonElement    Data);
