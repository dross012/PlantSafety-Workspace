// ============================================================================
// File: AdministrationCommandRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Internal Presentation-layer command request model for Administration CrossDomain,
//              decoupled from the external Contracts/Requests DTO.
// ============================================================================
//
// Onboarding Notes:
// - Created by the controller from the validated Contracts/Requests DTO. Passed to the MediatR
//   command constructor after optional enrichment.
//
// Rationale:
// - Decoupling allows the Presentation layer to enrich requests (e.g. resolved TenantId from
//   TenantContext) before dispatching to the Application layer.
//
// Future Extension Seams:
// - Add server-side enrichment fields here (e.g. CallerUserId from JWT) that should not be
//   exposed in the external contract.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Administration.Models.Requests;

/// <summary>Internal Presentation-layer command request model for Administration CrossDomain operations.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT by TenantResolutionMiddleware.</param>
/// <param name="SiteId">Optional site scope.</param>
/// <param name="Name">Record name.</param>
/// <param name="Description">Optional description.</param>
/// <param name="Status">Current status.</param>
public sealed record AdministrationCommandRequest(
    Guid    TenantId,
    Guid?   SiteId,
    string  Name,
    string? Description,
    string? Status);
