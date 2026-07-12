// ============================================================================
// File: CreateCorrectiveActionRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing request DTO for POST /features/continuous-improvement/capa,
//              bound from the HTTP request body.
// ============================================================================

namespace PlantSafety.Api.Features.ContinuousImprovement.CAPA.Contracts.Requests;

/// <summary>Request body for creating a new Corrective or Preventive Action.</summary>
/// <param name="SiteId">Site where the action is to be implemented.</param>
/// <param name="Title">Title. Max 200 characters.</param>
/// <param name="Description">Description. Max 4000 characters.</param>
/// <param name="ActionType">Action type: Corrective or Preventive.</param>
/// <param name="SourceEventId">Source event identifier (Incident, NearMiss, AuditFinding, etc.).</param>
/// <param name="SourceEventType">Source event type.</param>
/// <param name="Priority">Priority (Low, Medium, High, Critical).</param>
/// <param name="AssignedToWorkerId">Worker assigned to implement the action.</param>
/// <param name="DueAtUtc">Due date (UTC).</param>
public sealed record CreateCorrectiveActionRequest(
    Guid           SiteId,
    string         Title,
    string         Description,
    string         ActionType,
    Guid           SourceEventId,
    string         SourceEventType,
    string         Priority,
    Guid           AssignedToWorkerId,
    DateTimeOffset DueAtUtc);
