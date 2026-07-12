// ============================================================================
// File: UpdateAllInvestigationsRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External PUT request DTO for updating an existing AllInvestigations record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body along with the route id. Controller maps to UpdateAllInvestigationsCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command type.
//
// Future Extension Seams:
// - Add version token here when optimistic concurrency is required for AllInvestigations.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.AllInvestigations.Contracts.Requests;

/// <summary>Request body for updating an existing AllInvestigations record.</summary>
/// <param name="IncidentReference">Updated reference identifier of the related safety incident.</param>
/// <param name="AiConfidenceScore">Updated ai model confidence score (0.0-1.0) for the investigation findings.</param>
/// <param name="Summary">Updated ai-generated summary of the investigation.</param>
/// <param name="AssignedTo">Updated identity of the user or process assigned to the investigation.</param>
/// <param name="Status">Updated status value.</param>
public sealed record UpdateAllInvestigationsRequest(
    string   IncidentReference,
    double   AiConfidenceScore,
    string?   Summary,
    Guid?    AssignedTo,
    string   Status);