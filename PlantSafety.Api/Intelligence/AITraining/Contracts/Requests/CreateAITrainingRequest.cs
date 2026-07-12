// ============================================================================
// File: CreateAITrainingRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new AITraining record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateAITrainingCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add additional metadata fields here as the AITraining record schema evolves.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.AITraining.Contracts.Requests;

/// <summary>Request body for creating a new AITraining record.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="ModelName">Name of the model being trained.</param>
/// <param name="ModelVersion">Version identifier of the trained model artifact.</param>
/// <param name="DatasetReference">Reference to the training dataset used for this run.</param>
/// <param name="InitiatedBy">Identity of the user or process that initiated the training job.</param>
public sealed record CreateAITrainingRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   ModelName,
    string   ModelVersion,
    string?   DatasetReference,
    Guid?    InitiatedBy);