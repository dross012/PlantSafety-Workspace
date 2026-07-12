// ============================================================================
// File: AuditingCommandRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Internal Presentation-layer request models for Integrations.Auditing queries and commands.
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

namespace PlantSafety.Api.Integrations.Auditing.Models.Requests;

/// <summary>Internal Presentation-layer command request for Integrations.Auditing create/update operations.</summary>
/// <param name="TenantId">Resolved tenant identifier.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="ExternalSystem">Name of the external system involved in the call.</param>
/// <param name="Direction">Direction of the call (Inbound/Outbound).</param>
/// <param name="Endpoint">Endpoint or operation invoked.</param>
public sealed record AuditingCommandRequest(
    Guid   TenantId,
    Guid   SiteId,
    string ExternalSystem,
    string Direction,
    string Endpoint);
