// ============================================================================
// File: EngagementQueryRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Internal Presentation-layer request model for Engagement queries, decoupled from the external Contracts/Requests DTO.
// ============================================================================
//
// Onboarding Notes:
// - Created by the controller from the validated Contracts/Requests DTO. Passed to the MediatR query constructor after optional enrichment.
//
// Rationale:
// - Decoupling allows the Presentation layer to enrich requests (e.g. resolved TenantId from TenantContext) before dispatching to the Application layer.
//
// Future Extension Seams:
// - Add server-side enrichment fields here (e.g. CallerUserId from JWT) that should not be exposed in the external contract.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Engagement.Models.Requests;

/// <summary>Internal Presentation-layer request model for Engagement analytics queries.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT by TenantResolutionMiddleware.</param>
/// <param name="SiteId">Optional site filter.</param>
/// <param name="From">Start of date range (UTC).</param>
/// <param name="To">End of date range (UTC).</param>
public sealed record EngagementQueryRequest(
    Guid     TenantId,
    Guid?    SiteId,
    DateTime From,
    DateTime To);