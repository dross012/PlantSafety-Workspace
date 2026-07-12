// ============================================================================
// File: TextingCommandRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Internal Presentation-layer request models for Texting queries and commands.
// ============================================================================
//
// Onboarding Notes:
// - Created by the controller from validated Contracts/Requests DTOs. Passed to Application-layer query/command constructors.
//
// Rationale:
// - Decouples the external HTTP contract from Application-layer types and allows Presentation-layer enrichment.
//
// Future Extension Seams:
// - Add CallerUserId or resolved permission context here when commands require Presentation-layer enrichment.
//
// ============================================================================
namespace PlantSafety.Api.Integrations.Notifications.Texting.Models.Requests;

/// <summary>Internal Presentation-layer command request for Texting create/update operations.</summary>
/// <param name="TenantId">Resolved tenant identifier.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="RecipientPhoneNumber">Phone number the notification SMS was delivered to.</param>
/// <param name="MessageBody">Text content of the notification SMS.</param>
/// <param name="TriggerEvent">Name of the platform event that triggered the SMS notification.</param>
/// <param name="SentBy">Identity of the user or process that triggered the notification.</param>
public sealed record TextingCommandRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   RecipientPhoneNumber,
    string   MessageBody,
    string?   TriggerEvent,
    Guid?    SentBy);