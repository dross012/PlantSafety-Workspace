// ============================================================================
// File: EmailCommandRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Internal Presentation-layer request models for Email queries and commands.
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
namespace PlantSafety.Api.Integrations.Notifications.Email.Models.Requests;

/// <summary>Internal Presentation-layer command request for Email create/update operations.</summary>
/// <param name="TenantId">Resolved tenant identifier.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="RecipientAddress">Email address the notification was delivered to.</param>
/// <param name="Subject">Subject line of the notification email.</param>
/// <param name="TemplateName">Name of the email template used to render the notification.</param>
/// <param name="SentBy">Identity of the user or process that triggered the notification.</param>
public sealed record EmailCommandRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   RecipientAddress,
    string   Subject,
    string?   TemplateName,
    Guid?    SentBy);