// ============================================================================
// File: SecretRotationEventGridPayload.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-03
// Description: Minimal shape of an Azure Event Grid event as delivered for a Key Vault
//   "SecretNearExpiry"/"SecretExpired"/rotation-style notification (Event Grid's generic
//   schema: https://learn.microsoft.com/azure/event-grid/event-schema). Only the fields
//   this webhook actually consumes are modelled -- this is not a full Event Grid SDK
//   contract, since PlantSafety does not take an Event Grid SDK dependency for one webhook.
// ============================================================================
namespace PlantSafety.Api.Operations.Secrets.Models;

/// <summary>
/// Minimal Azure Event Grid event envelope for Key Vault secret-rotation notifications.
/// </summary>
/// <param name="Id">Event Grid event identifier.</param>
/// <param name="EventType">
/// Event Grid event type, e.g. "Microsoft.KeyVault.SecretNewVersionCreated" (the closest
/// real Key Vault event to "rotation occurred").
/// </param>
/// <param name="Subject">The Key Vault secret's resource path (Event Grid's "subject" field).</param>
/// <param name="EventTime">UTC timestamp Event Grid assigned to the event.</param>
/// <param name="Data">Event-specific data payload (Key Vault's ObjectName is the secret name).</param>
public sealed record SecretRotationEventGridPayload(
    string? Id,
    string? EventType,
    string? Subject,
    DateTime? EventTime,
    SecretRotationEventGridData? Data);

/// <summary>Key Vault-specific Event Grid data payload for secret events.</summary>
/// <param name="ObjectName">The rotated secret's name within the vault.</param>
/// <param name="VaultName">The Key Vault name the secret lives in.</param>
public sealed record SecretRotationEventGridData(string? ObjectName, string? VaultName);

/// <summary>
/// Azure Event Grid's subscription-validation handshake event
/// ("Microsoft.EventGrid.SubscriptionValidationEvent"), which every new webhook endpoint
/// must echo back before Event Grid will start delivering real events to it.
/// </summary>
/// <param name="ValidationCode">The code Event Grid expects echoed back in the response.</param>
/// <param name="ValidationUrl">Alternative validation URL Event Grid may expect a GET against instead.</param>
public sealed record EventGridSubscriptionValidationData(string? ValidationCode, string? ValidationUrl);
