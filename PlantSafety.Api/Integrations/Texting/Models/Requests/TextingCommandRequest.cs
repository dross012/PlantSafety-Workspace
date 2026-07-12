// ============================================================================
// File: TextingCommandRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Internal Presentation-layer request models for Integrations.Texting queries and commands.
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

namespace PlantSafety.Api.Integrations.Texting.Models.Requests;

/// <summary>Internal Presentation-layer command request for Integrations.Texting create/update operations.</summary>
/// <param name="TenantId">Resolved tenant identifier.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="RecipientPhoneNumber">Destination phone number in E.164 format.</param>
/// <param name="Description">Detailed description.</param>
/// <param name="ScheduledAtUtc">Scheduled date/time (UTC).</param>
/// <param name="AssignedToUserId">Optional assignee.</param>
public sealed record TextingCommandRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   RecipientPhoneNumber,
    string?  Description,
    DateTime ScheduledAtUtc,
    Guid?    AssignedToUserId);
