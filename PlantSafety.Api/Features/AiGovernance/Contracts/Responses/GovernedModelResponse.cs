// ============================================================================
// File: GovernedModelResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing response DTOs for the AI Governance module.
// ============================================================================

namespace PlantSafety.Api.Features.AiGovernance.Contracts.Responses;

/// <summary>Response payload for a single governed AI/ML model registry entry.</summary>
public sealed record GovernedModelResponse(
    Guid     ModelId,
    Guid     TenantId,
    Guid?    SiteId,
    string   ModelName,
    string   ModelVersion,
    string   GovernanceStatus,
    string   RiskTier,
    string   DriftStatus,
    DateTime LastReviewedAtUtc,
    string?  ReviewerName);

/// <summary>Envelope response payload for the governed model listing.</summary>
public sealed record AiGovernanceListResponse(
    Guid                                 TenantId,
    Guid?                                SiteId,
    DateTime                             GeneratedAt,
    IReadOnlyList<GovernedModelResponse> Items);

/// <summary>Response payload returned after registering a governed model.</summary>
/// <param name="ModelId">Identifier of the newly registered model.</param>
public sealed record RegisterGovernedModelResponse(Guid ModelId);
