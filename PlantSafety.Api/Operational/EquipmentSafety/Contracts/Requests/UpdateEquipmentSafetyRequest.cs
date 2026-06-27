// ============================================================================
// File: UpdateEquipmentSafetyRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: External PUT DTO for updating an existing EquipmentSafety Operational record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from request body with route id. Controller maps to UpdateEquipmentSafetyCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command.
//
// Future Extension Seams:
// - Add version token for optimistic concurrency when required.
//
// ============================================================================

namespace PlantSafety.Api.Operational.EquipmentSafety.Contracts.Requests;

/// <summary>Request body for updating an existing EquipmentSafety Operational record.</summary>
/// <param name="Title">Updated title.</param>
/// <param name="Description">Updated description.</param>
/// <param name="ScheduledDate">Updated scheduled date (UTC).</param>
/// <param name="AssignedToUserId">Updated assignee.</param>
/// <param name="Status">Updated status.</param>
public sealed record UpdateEquipmentSafetyRequest(
    string   Title,
    string?  Description,
    DateTime ScheduledDate,
    Guid?    AssignedToUserId,
    string   Status);