// ============================================================================
// File: InternalCommandRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Internal Presentation-layer request models for Internal queries and commands.
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
namespace PlantSafety.Api.Integrations.Notifications.Internal.Models.Requests;

/// <summary>Internal Presentation-layer command request for Internal create/update operations.</summary>
/// <param name="TenantId">Resolved tenant identifier.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="RecipientUserId">User the internal notification was delivered to.</param>
/// <param name="Message">Notification message body.</param>
/// <param name="Category">Notification category (e.g., Alert/Reminder/Info).</param>
/// <param name="TriggeredBy">Identity of the user or process that triggered the notification.</param>
public sealed record InternalCommandRequest(
    Guid     TenantId,
    Guid     SiteId,
    Guid   RecipientUserId,
    string   Message,
    string?   Category,
    Guid?    TriggeredBy);