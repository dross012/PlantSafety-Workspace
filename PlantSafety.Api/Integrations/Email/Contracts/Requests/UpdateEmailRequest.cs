// ============================================================================
// File: UpdateEmailRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External PUT request DTO for updating an existing email delivery record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body along with the route id. Controller maps to UpdateEmailCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command type.
//
// Future Extension Seams:
// - Add version token here when optimistic concurrency is required for Email records.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Email.Contracts.Requests;

/// <summary>Request body for updating an existing email delivery record.</summary>
/// <param name="Subject">Updated subject line.</param>
/// <param name="Status">Updated delivery status value.</param>
public sealed record UpdateEmailRequest(
    string Subject,
    string Status);
