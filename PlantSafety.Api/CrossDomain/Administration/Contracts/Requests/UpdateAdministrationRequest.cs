// ============================================================================
// File: UpdateAdministrationRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: External PUT body DTO for updating an Administration CrossDomain record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the HTTP request body via [FromBody] in AdministrationV1Controller.UpdateAsync.
//
// Rationale:
// - Decouples the external HTTP contract from Application-layer command types.
//
// Future Extension Seams:
// - Add patch-style partial update support via PATCH + JsonPatchDocument when required.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Administration.Contracts.Requests;

/// <summary>Payload for updating an existing Administration CrossDomain record.</summary>
/// <param name="Name">Updated name.</param>
/// <param name="Description">Updated description.</param>
/// <param name="Status">Updated status value.</param>
public sealed record UpdateAdministrationRequest(
    string  Name,
    string? Description,
    string  Status);
