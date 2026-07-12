// ============================================================================
// File: CreateAllInvestigationsRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new AllInvestigations record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateAllInvestigationsCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add additional metadata fields here as the AllInvestigations record schema evolves.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.AllInvestigations.Contracts.Requests;

/// <summary>Request body for creating a new AllInvestigations record.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="IncidentReference">Reference identifier of the related safety incident.</param>
/// <param name="AiConfidenceScore">AI model confidence score (0.0-1.0) for the investigation findings.</param>
/// <param name="Summary">AI-generated summary of the investigation.</param>
/// <param name="AssignedTo">Identity of the user or process assigned to the investigation.</param>
public sealed record CreateAllInvestigationsRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   IncidentReference,
    double   AiConfidenceScore,
    string?   Summary,
    Guid?    AssignedTo);