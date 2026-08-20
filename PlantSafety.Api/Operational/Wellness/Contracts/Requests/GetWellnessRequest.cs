// ============================================================================
// File: GetWellnessRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: External-facing DTO for GET /operational/wellness query parameters, bound from query string by the V1 controller.
// ============================================================================
//
// Onboarding Notes:
// - Validated by FluentValidation via ApiValidationFilter before MediatR dispatch.
//
// Rationale:
// - Decouples the external HTTP contract from internal Application-layer query types.
//
// Future Extension Seams:
// - Add cursor/page fields here when Wellness endpoints need server-side paging.
//
// ============================================================================

namespace PlantSafety.Api.Operational.Wellness.Contracts.Requests;

/// <summary>Query parameters for retrieving Wellness data.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC). Defaults to UtcNow when not supplied.</param>
public sealed record GetWellnessRequest(
    Guid?     SiteId,
    DateTime  From,
    DateTime? To);