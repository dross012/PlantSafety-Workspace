// ============================================================================
// File: RegisterGovernedModelRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing request DTO for POST v1/ai-governance/models,
//              bound from the HTTP request body.
// ============================================================================

namespace PlantSafety.Api.Features.AiGovernance.Contracts.Requests;

/// <summary>Request body for registering a new governed AI/ML model.</summary>
public sealed record RegisterGovernedModelRequest(
    Guid?  SiteId,
    string ModelName,
    string ModelVersion,
    string RiskTier);
