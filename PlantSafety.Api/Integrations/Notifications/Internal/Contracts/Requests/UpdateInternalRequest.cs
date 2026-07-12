// ============================================================================
// File: UpdateInternalRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External PUT request DTO for updating an existing Internal record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body along with the route id. Controller maps to UpdateInternalCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command type.
//
// Future Extension Seams:
// - Add version token here when optimistic concurrency is required for Internal.
//
// ============================================================================
namespace PlantSafety.Api.Integrations.Notifications.Internal.Contracts.Requests;

/// <summary>Request body for updating an existing Internal record.</summary>
/// <param name="RecipientUserId">Updated user the internal notification was delivered to.</param>
/// <param name="Message">Updated notification message body.</param>
/// <param name="Category">Updated notification category (e.g., alert/reminder/info).</param>
/// <param name="TriggeredBy">Updated identity of the user or process that triggered the notification.</param>
/// <param name="Status">Updated status value.</param>
public sealed record UpdateInternalRequest(
    Guid   RecipientUserId,
    string   Message,
    string?   Category,
    Guid?    TriggeredBy,
    string   Status);