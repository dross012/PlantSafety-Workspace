// ============================================================================
// File: CreateAdministrationRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: External POST body DTO for creating an Administration CrossDomain record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the HTTP request body via [FromBody] in AdministrationV1Controller.CreateAsync.
//
// Rationale:
// - Decouples the external HTTP contract from Application-layer command types.
//
// Future Extension Seams:
// - Add additional fields as the Administration domain evolves.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Administration.Contracts.Requests;

/// <summary>Payload for creating a new Administration CrossDomain record.</summary>
/// <param name="TenantId">Owning tenant identifier.</param>
/// <param name="SiteId">Optional site scope.</param>
/// <param name="Name">Record name.</param>
/// <param name="Description">Optional description.</param>
public sealed record CreateAdministrationRequest(
    Guid    TenantId,
    Guid?   SiteId,
    string  Name,
    string? Description);
