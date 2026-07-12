// ============================================================================
// File: UpdateImportRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External PUT request DTO for updating an existing Integrations.Import record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body along with the route id. Controller maps to UpdateImportCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command type.
//
// Future Extension Seams:
// - Add version token here when optimistic concurrency is required for Integrations.Import.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Import.Contracts.Requests;

/// <summary>Request body for updating an existing Integrations.Import record.</summary>
/// <param name="SourceSystem">Updated SourceSystem.</param>
/// <param name="Description">Updated description.</param>
/// <param name="ScheduledAtUtc">Updated scheduled date/time (UTC).</param>
/// <param name="AssignedToUserId">Updated assignee.</param>
/// <param name="Status">Updated status value.</param>
public sealed record UpdateImportRequest(
    string   SourceSystem,
    string?  Description,
    DateTime ScheduledAtUtc,
    Guid?    AssignedToUserId,
    string   Status);
