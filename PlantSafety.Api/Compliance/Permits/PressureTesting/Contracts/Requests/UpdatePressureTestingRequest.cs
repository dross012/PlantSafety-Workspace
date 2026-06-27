// ============================================================================
// File: UpdatePressureTestingRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: External PUT request DTO for updating an existing PressureTesting compliance record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body along with the route id. Controller maps to UpdatePressureTestingCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command type.
//
// Future Extension Seams:
// - Add version token here when optimistic concurrency is required for PressureTesting.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.PressureTesting.Contracts.Requests;

/// <summary>Request body for updating an existing PressureTesting compliance record.</summary>
/// <param name="Title">Updated title.</param>
/// <param name="Description">Updated description.</param>
/// <param name="ScheduledDate">Updated scheduled date (UTC).</param>
/// <param name="AssignedToUserId">Updated assignee.</param>
/// <param name="Status">Updated status value.</param>
public sealed record UpdatePressureTestingRequest(
    string   Title,
    string?  Description,
    DateTime ScheduledDate,
    Guid?    AssignedToUserId,
    string   Status);