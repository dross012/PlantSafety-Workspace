// ============================================================================
// File: SetUserFeatureFlagOverrideRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-03
// Description: Request body for PUT /api/v1/operations/configuration/feature-flags/{flagName}/user-overrides/{userId}.
// ============================================================================
namespace PlantSafety.Api.Operations.Configuration.Contracts.Requests;

/// <summary>Request body to create/update a user-level feature flag override.</summary>
/// <param name="Enabled">Whether the feature is enabled for this specific user.</param>
public sealed record SetUserFeatureFlagOverrideRequest(bool Enabled);
