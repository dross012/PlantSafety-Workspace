// ============================================================================
// File: AllInvestigationsCommandRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Internal Presentation-layer request models for AllInvestigations queries and commands.
// ============================================================================
//
// Onboarding Notes:
// - Created by the controller from validated Contracts/Requests DTOs. Passed to Application-layer query/command constructors.
//
// Rationale:
// - Decouples the external HTTP contract from Application-layer types and allows Presentation-layer enrichment.
//
// Future Extension Seams:
// - Add CallerUserId or resolved permission context here when commands require Presentation-layer enrichment.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.AllInvestigations.Models.Requests;

/// <summary>Internal Presentation-layer command request for AllInvestigations create/update operations.</summary>
/// <param name="TenantId">Resolved tenant identifier.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="IncidentReference">Reference identifier of the related safety incident.</param>
/// <param name="AiConfidenceScore">AI model confidence score (0.0-1.0) for the investigation findings.</param>
/// <param name="Summary">AI-generated summary of the investigation.</param>
/// <param name="AssignedTo">Identity of the user or process assigned to the investigation.</param>
public sealed record AllInvestigationsCommandRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   IncidentReference,
    double   AiConfidenceScore,
    string?   Summary,
    Guid?    AssignedTo);