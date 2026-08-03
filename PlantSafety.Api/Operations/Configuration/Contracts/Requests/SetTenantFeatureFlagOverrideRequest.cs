// ============================================================================
// File: SetTenantFeatureFlagOverrideRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-03
// Description: Request body for PUT /api/v1/operations/configuration/feature-flags/{flagName}/tenant-overrides/{tenantId}.
// ============================================================================
namespace PlantSafety.Api.Operations.Configuration.Contracts.Requests;

/// <summary>Request body to create/update a tenant-level feature flag override.</summary>
/// <param name="Enabled">Whether the feature is enabled for this tenant.</param>
/// <param name="RolloutPercentage">Optional tenant-scoped rollout percentage (0-100), or null to clear it.</param>
public sealed record SetTenantFeatureFlagOverrideRequest(bool Enabled, int? RolloutPercentage);
