// ============================================================================
// File: AllInvestigationsQueryRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Internal Presentation-layer request model for AllInvestigations queries, decoupled from the external Contracts/Requests DTO.
// ============================================================================
//
// Onboarding Notes:
// - Created by the controller from the validated Contracts/Requests DTO.
//
// Rationale:
// - Decoupling allows the Presentation layer to enrich requests before dispatching to the Application layer.
//
// Future Extension Seams:
// - Add server-side enrichment fields here (e.g. CallerUserId from JWT) that should not be exposed in the external contract.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.AllInvestigations.Models.Requests;

/// <summary>Internal Presentation-layer request model for AllInvestigations queries.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT by TenantResolutionMiddleware.</param>
/// <param name="SiteId">Optional site filter.</param>
/// <param name="From">Start of date range (UTC).</param>
/// <param name="To">End of date range (UTC).</param>
public sealed record AllInvestigationsQueryRequest(
    Guid     TenantId,
    Guid?    SiteId,
    DateTime From,
    DateTime To);