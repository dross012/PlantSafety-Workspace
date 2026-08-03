// ============================================================================
// File: GetISO14001Request.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: External-facing DTO for GET /compliance/certification/iso14001 query parameters, bound from query string by the V1 controller.
// ============================================================================
//
// Onboarding Notes:
// - Validated by FluentValidation via ApiValidationFilter before MediatR dispatch.
//
// Rationale:
// - Decouples the external HTTP contract from internal Application-layer query types.
//
// Future Extension Seams:
// - Add cursor/page fields here when ISO14001 endpoints need server-side paging.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Certification.ISO14001.Contracts.Requests;

/// <summary>Query parameters for retrieving ISO14001 data.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC). Defaults to UtcNow when not supplied.</param>
public sealed record GetISO14001Request(
    Guid?     SiteId,
    DateTime  From,
    DateTime? To);