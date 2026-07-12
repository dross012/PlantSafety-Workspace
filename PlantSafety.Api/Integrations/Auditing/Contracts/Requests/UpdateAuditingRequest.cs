// ============================================================================
// File: UpdateAuditingRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External PUT request DTO for updating an existing integration audit log entry.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body along with the route id. Controller maps to UpdateAuditingCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command type.
//
// Future Extension Seams:
// - Add version token here when optimistic concurrency is required for Auditing records.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Auditing.Contracts.Requests;

/// <summary>Request body for updating an existing integration audit log entry.</summary>
/// <param name="Endpoint">Updated endpoint or operation.</param>
/// <param name="Status">Updated outcome status value.</param>
public sealed record UpdateAuditingRequest(
    string Endpoint,
    string Status);
