// ============================================================================
// File: AnalyticsCommandRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Internal Presentation-layer request models for Integrations.Analytics queries and commands.
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

namespace PlantSafety.Api.Integrations.Analytics.Models.Requests;

/// <summary>Internal Presentation-layer command request for Integrations.Analytics create/update operations.</summary>
/// <param name="TenantId">Resolved tenant identifier.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="SourceSystem">Name of the external analytics system the record was ingested from.</param>
/// <param name="MetricName">Name of the ingested analytics metric.</param>
/// <param name="MetricValue">Value of the ingested analytics metric.</param>
public sealed record AnalyticsCommandRequest(
    Guid    TenantId,
    Guid    SiteId,
    string  SourceSystem,
    string  MetricName,
    decimal MetricValue);
