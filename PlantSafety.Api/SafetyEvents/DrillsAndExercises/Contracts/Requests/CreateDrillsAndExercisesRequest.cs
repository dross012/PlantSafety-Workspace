// ============================================================================
// File: CreateDrillsAndExercisesRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: External POST DTO for creating a new DrillsAndExercises SafetyEvents record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from request body. Validated by FluentValidation via ApiValidationFilter.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add file attachment metadata when DrillsAndExercises records support document uploads.
//
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.DrillsAndExercises.Contracts.Requests;

/// <summary>Request body for creating a new DrillsAndExercises SafetyEvents record.</summary>
/// <param name="TenantId">Owning tenant.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="Title">Short descriptive title.</param>
/// <param name="Description">Detailed notes.</param>
/// <param name="ScheduledDate">Scheduled activity date (UTC).</param>
/// <param name="AssignedToUserId">Optional assignee.</param>
public sealed record CreateDrillsAndExercisesRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   Title,
    string?  Description,
    DateTime ScheduledDate,
    Guid?    AssignedToUserId);