// ============================================================================
// File: GetEngagementRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: External-facing DTO for GET /engagement query parameters, bound from query string by the V1 controller.
// ============================================================================
//
// Onboarding Notes:
// - Validated by FluentValidation via ApiValidationFilter before MediatR dispatch. Kept separate from Models/Requests to decouple the external contract from internal query objects.
//
// Rationale:
// - Decouples the external HTTP contract from internal Application-layer query types. The controller maps this to the internal query object.
//
// Future Extension Seams:
// - Add cursor/page fields here when Engagement endpoints need server-side paging.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Engagement.Contracts.Requests;

/// <summary>Query parameters for retrieving Engagement analytics data.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims by TenantResolutionMiddleware.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC). Defaults to UtcNow when not supplied.</param>
public sealed record GetEngagementRequest(
    Guid      TenantId,
    Guid?     SiteId,
    DateTime  From,
    DateTime? To);