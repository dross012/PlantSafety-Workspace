// ============================================================================
// File: GetAuditingRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External-facing DTO for GET /integrations/auditing query parameters, bound from query string by the V1 controller.
// ============================================================================
//
// Onboarding Notes:
// - Validated by FluentValidation via ApiValidationFilter before MediatR dispatch.
//
// Rationale:
// - Decouples the external HTTP contract from internal Application-layer query types.
//
// Future Extension Seams:
// - Add cursor/page fields here when Auditing endpoints need server-side paging.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Auditing.Contracts.Requests;

/// <summary>Query parameters for retrieving Integrations.Auditing data.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims by TenantResolutionMiddleware.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC). Defaults to UtcNow when not supplied.</param>
public sealed record GetAuditingRequest(
    Guid      TenantId,
    Guid?     SiteId,
    DateTime  From,
    DateTime? To);
