// ============================================================================
// File: UpdateAnalyzersRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External PUT request DTO for updating an existing analyzer run result.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body along with the route id. Controller maps to UpdateAnalyzersCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command type.
//
// Future Extension Seams:
// - Add version token here when optimistic concurrency is required for Analyzers records.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Analyzers.Contracts.Requests;

/// <summary>Request body for updating an existing analyzer run result.</summary>
/// <param name="FindingsCount">Updated findings count.</param>
/// <param name="Status">Updated outcome status value.</param>
public sealed record UpdateAnalyzersRequest(
    int    FindingsCount,
    string Status);
