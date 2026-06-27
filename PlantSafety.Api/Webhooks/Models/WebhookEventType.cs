// ============================================================================
// File: WebhookEventType.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Enumeration of all inbound webhook event types that PlantSafety.Api
//              can receive from external systems via the webhook receiver endpoint.
// ============================================================================
//
// Onboarding Notes:
// - Used in InboundWebhookPayload.EventType to allow the webhook receiver controller
//   and the Application layer to dispatch on a strongly-typed event discriminator
//   rather than a raw string.
// - The enum values correspond to event type strings in the webhook contract
//   documented in the PlantSafety Architecture Specification (Integration Events).
// - "Inbound" events are webhooks received FROM external systems (IoT platforms,
//   ERP systems, HR systems). "Outbound" webhooks sent FROM PlantSafety to
//   subscribers are dispatched by the Infrastructure layer and are not represented here.
// - Adding a new external system integration? Add its event types here first,
//   then add the corresponding case in the Application layer command handler.
//
// Rationale:
// - A typed enum prevents the Application layer command handler from depending on
//   raw strings for dispatch, and gives exhaustiveness checking in switch expressions.
//
// ============================================================================

namespace PlantSafety.Api.Webhooks.Models;

/// <summary>
/// Inbound webhook event types received from external systems via
/// <c>POST api/v1/webhooks/receive</c>.
/// </summary>
public enum WebhookEventType
{
    // ── IoT / Sensor events ───────────────────────────────────────────────────

    /// <summary>A sensor threshold was breached (gas, temperature, pressure).</summary>
    SensorThresholdBreached,

    /// <summary>An IoT device went offline or lost connectivity.</summary>
    DeviceOffline,

    // ── HR system events ──────────────────────────────────────────────────────

    /// <summary>A worker was onboarded in the HR system and needs a PlantSafety profile.</summary>
    WorkerOnboarded,

    /// <summary>A worker was terminated in the HR system and needs access revocation.</summary>
    WorkerTerminated,

    /// <summary>A worker completed a certification in the HR / LMS system.</summary>
    CertificationCompleted,

    // ── ERP / Work order events ───────────────────────────────────────────────

    /// <summary>A permit-to-work was issued in the ERP system.</summary>
    PermitToWorkIssued,

    /// <summary>A permit-to-work was cancelled in the ERP system.</summary>
    PermitToWorkCancelled,

    /// <summary>A maintenance work order was closed in the ERP system.</summary>
    WorkOrderClosed,

    // ── External compliance / audit events ────────────────────────────────────

    /// <summary>An external audit finding was recorded by a third-party auditor.</summary>
    ExternalAuditFindingReceived,

    /// <summary>A regulatory submission was acknowledged by the regulatory body.</summary>
    RegulatorySubmissionAcknowledged,

    // ── Platform lifecycle events ─────────────────────────────────────────────

    /// <summary>A new tenant was provisioned in the identity provider.</summary>
    TenantProvisioned,

    /// <summary>Unknown or unrecognised event type. Logged and discarded.</summary>
    Unknown
}
