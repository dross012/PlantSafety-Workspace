// ============================================================================
// File: WebhookAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Authorization policy name constants for the Webhooks area of
//              PlantSafety.Api. The receiver endpoint uses HMAC signature
//              verification rather than JWT, so these policies are used for
//              the webhook management endpoints (register, list, delete).
// ============================================================================
//
// Onboarding Notes:
// - The inbound WebhookReceiverV1Controller.ReceiveAsync action is [AllowAnonymous]
//   because it is secured by HMAC-SHA256 signature verification, not JWT.
//   Do NOT add [Authorize] to that specific action.
// - ManageWebhooks policy is used on any future webhook management endpoints
//   (e.g. GET /webhooks to list registered subscriptions, DELETE /webhooks/{id}).
//   These endpoints ARE JWT-secured and require the platform admin role.
//
// Rationale:
// - Keeping policy name constants here (rather than inline strings in controllers)
//   ensures consistency between attribute decoration and policy registration in
//   AuthorizationPolicyExtensions.
//
// ============================================================================

namespace PlantSafety.Api.Webhooks.Authorization;

/// <summary>
/// Authorization policy name constants for the PlantSafety.Api Webhooks area.
/// </summary>
public static class WebhookAuthorizationPolicies
{
    /// <summary>
    /// Policy required to manage webhook subscriptions (register, list, delete).
    /// Not required on the inbound receiver endpoint, which uses HMAC verification.
    /// </summary>
    public const string ManageWebhooks = "Webhooks.Manage";

    /// <summary>
    /// Policy required to view webhook delivery logs and retry history.
    /// </summary>
    public const string ViewWebhookLogs = "Webhooks.ViewLogs";
}
