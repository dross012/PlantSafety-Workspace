// ============================================================================
// File: AITrainingCommandRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Internal Presentation-layer request models for AITraining queries and commands.
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
namespace PlantSafety.Api.Intelligence.AITraining.Models.Requests;

/// <summary>Internal Presentation-layer command request for AITraining create/update operations.</summary>
/// <param name="TenantId">Resolved tenant identifier.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="ModelName">Name of the model being trained.</param>
/// <param name="ModelVersion">Version identifier of the trained model artifact.</param>
/// <param name="DatasetReference">Reference to the training dataset used for this run.</param>
/// <param name="InitiatedBy">Identity of the user or process that initiated the training job.</param>
public sealed record AITrainingCommandRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   ModelName,
    string   ModelVersion,
    string?   DatasetReference,
    Guid?    InitiatedBy);