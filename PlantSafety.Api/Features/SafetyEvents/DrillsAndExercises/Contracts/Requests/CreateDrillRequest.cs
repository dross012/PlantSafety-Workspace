// ============================================================================
// File: CreateDrillRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for POST /features/safety-events/drills,
//              bound from the request body by the V1 controller.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises.Contracts.Requests;

/// <summary>Request body for scheduling a Safety Drill or Exercise.</summary>
/// <param name="SiteId">Site where the drill will be conducted.</param>
/// <param name="Title">Short title. Max 200 characters.</param>
/// <param name="Description">Description of objectives and scope. Max 4000 characters.</param>
/// <param name="DrillType">Drill type (e.g., Evacuation, FireResponse, ChemicalSpill, MedicalEmergency).</param>
/// <param name="ScheduledAtUtc">UTC timestamp when the drill is scheduled to occur.</param>
/// <param name="CoordinatorId">Drill coordinator.</param>
public sealed record CreateDrillRequest(
    Guid           SiteId,
    string         Title,
    string         Description,
    string         DrillType,
    DateTimeOffset ScheduledAtUtc,
    Guid           CoordinatorId);
