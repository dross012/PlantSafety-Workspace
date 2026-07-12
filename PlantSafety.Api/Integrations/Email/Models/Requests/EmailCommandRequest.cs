// ============================================================================
// File: EmailCommandRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Internal Presentation-layer request models for Integrations.Email queries and commands.
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

namespace PlantSafety.Api.Integrations.Email.Models.Requests;

/// <summary>Internal Presentation-layer command request for Integrations.Email create/update operations.</summary>
/// <param name="TenantId">Resolved tenant identifier.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="Recipient">Recipient email address.</param>
/// <param name="Subject">Subject line of the email.</param>
/// <param name="Provider">External email gateway/provider used for delivery (e.g., SendGrid, SMTP relay).</param>
public sealed record EmailCommandRequest(
    Guid   TenantId,
    Guid   SiteId,
    string Recipient,
    string Subject,
    string Provider);
