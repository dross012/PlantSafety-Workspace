// ============================================================================
// File: SetFeatureFlagEnabledRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-20
// Description: Request body for PATCH /api/v1/operations/configuration/{databaseKey}/feature-flags/{flagName}.
// ============================================================================
namespace PlantSafety.Api.Operations.Configuration.Contracts.Requests;

/// <summary>Request body to toggle a single feature flag's IsEnabled value.</summary>
/// <param name="IsEnabled">The new IsEnabled value.</param>
public sealed record SetFeatureFlagEnabledRequest(bool IsEnabled);
