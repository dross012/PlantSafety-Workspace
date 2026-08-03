// ============================================================================
// File: SetFeatureFlagRolloutPercentageRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-03
// Description: Request body for PUT /api/v1/operations/configuration/{databaseKey}/feature-flags/{flagName}/rollout-percentage.
// ============================================================================
namespace PlantSafety.Api.Operations.Configuration.Contracts.Requests;

/// <summary>Request body to set (or clear) a single feature flag's canary-rollout percentage.</summary>
/// <param name="RolloutPercentage">The new rollout percentage (0-100), or null to clear it.</param>
public sealed record SetFeatureFlagRolloutPercentageRequest(int? RolloutPercentage);
