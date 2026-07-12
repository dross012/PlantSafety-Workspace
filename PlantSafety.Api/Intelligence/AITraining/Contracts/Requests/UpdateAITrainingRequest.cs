// ============================================================================
// File: UpdateAITrainingRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External PUT request DTO for updating an existing AITraining record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body along with the route id. Controller maps to UpdateAITrainingCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command type.
//
// Future Extension Seams:
// - Add version token here when optimistic concurrency is required for AITraining.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.AITraining.Contracts.Requests;

/// <summary>Request body for updating an existing AITraining record.</summary>
/// <param name="ModelName">Updated name of the model being trained.</param>
/// <param name="ModelVersion">Updated version identifier of the trained model artifact.</param>
/// <param name="DatasetReference">Updated reference to the training dataset used for this run.</param>
/// <param name="InitiatedBy">Updated identity of the user or process that initiated the training job.</param>
/// <param name="Status">Updated status value.</param>
public sealed record UpdateAITrainingRequest(
    string   ModelName,
    string   ModelVersion,
    string?   DatasetReference,
    Guid?    InitiatedBy,
    string   Status);