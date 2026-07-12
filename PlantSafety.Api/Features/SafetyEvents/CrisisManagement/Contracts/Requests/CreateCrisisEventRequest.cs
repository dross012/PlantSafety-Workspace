// ============================================================================
// File: CreateCrisisEventRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for POST /features/safety-events/crisis-events,
//              bound from the request body by the V1 controller.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.CrisisManagement.Contracts.Requests;

/// <summary>Request body for declaring a new Crisis Management event.</summary>
/// <param name="SiteId">Site affected.</param>
/// <param name="Title">Short title. Max 200 characters.</param>
/// <param name="Description">Detailed description. Max 4000 characters.</param>
/// <param name="CrisisCategory">Crisis category (e.g., Fire, ChemicalSpill, StructuralFailure, MassInjury).</param>
/// <param name="DeclaredAtUtc">UTC timestamp when the crisis was declared.</param>
/// <param name="IncidentCommanderId">Worker serving as Incident Commander.</param>
public sealed record CreateCrisisEventRequest(
    Guid           SiteId,
    string         Title,
    string         Description,
    string         CrisisCategory,
    DateTimeOffset DeclaredAtUtc,
    Guid           IncidentCommanderId);
