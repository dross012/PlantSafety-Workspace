// ============================================================================
// File: CreateEmergencyResponseRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for POST /features/safety-events/emergency-responses,
//              bound from the request body by the V1 controller.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.EmergencyManagement.Contracts.Requests;

/// <summary>Request body for activating an Emergency Response plan.</summary>
/// <param name="SiteId">Site where the emergency response is active.</param>
/// <param name="LinkedCrisisEventId">Optional linked crisis event.</param>
/// <param name="ResponsePlanName">Name of the emergency response plan activated.</param>
/// <param name="Description">Description of the emergency and response. Max 4000 characters.</param>
/// <param name="ActivatedAtUtc">UTC timestamp of activation.</param>
/// <param name="CoordinatorId">Response coordinator.</param>
public sealed record CreateEmergencyResponseRequest(
    Guid           SiteId,
    Guid?          LinkedCrisisEventId,
    string         ResponsePlanName,
    string         Description,
    DateTimeOffset ActivatedAtUtc,
    Guid           CoordinatorId);
