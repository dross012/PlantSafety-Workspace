// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           FeatureFlagDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    DB-level ops-controlled feature flag, sourced from
//                  [System].[FeatureFlag] (Phase 14 Governance, 144-feature-flags.sql).
//                  Confirmed present ONLY in PlantSafety.ArchiveDB and
//                  PlantSafety.IntegrationDB — each database's table is independent
//                  (no cross-database sync); flags are NOT globally shared. See
//                  IFeatureFlagRepository for the full rationale.
//
//                  RolloutPercentage added 2026-08-03 (Phase 6.6, §6.2 Blue/Green + Canary +
//                  Tenant Rollout, migration 190-feature-flag-rollout-percentage.sql). NULL
//                  means "no percentage rollout configured -- IsEnabled alone governs",
//                  matching every flag's behavior before this column existed. See
//                  PlantSafety.Application's Common\Rollout\FeatureRolloutEvaluator for how a
//                  non-NULL value is combined with a TenantId to deterministically decide
//                  rollout-wave membership.
//                  Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Configuration.Models;

/// <summary>
/// A single DB-level ops feature flag row from a specific database's
/// <c>[System].[FeatureFlag]</c> table.
/// </summary>
/// <param name="DatabaseKey">
/// The logical database key the flag lives in (e.g. "ArchiveDB", "IntegrationDB"), matching a
/// key in the Infrastructure layer's ConfigurationOptions.DatabaseConnectionStrings.
/// </param>
/// <param name="FlagName">The unique flag name (e.g. "RetentionPurge.Enabled").</param>
/// <param name="IsEnabled">Whether the flag is currently enabled.</param>
/// <param name="Description">Human-readable description of what the flag controls.</param>
/// <param name="UpdatedUtc">When the flag was last changed.</param>
/// <param name="UpdatedBy">Who last changed the flag.</param>
/// <param name="RolloutPercentage">
/// Optional canary-rollout percentage (0-100). NULL means no percentage rollout is
/// configured for this flag -- <see cref="IsEnabled"/> alone governs. When set, combine with
/// a TenantId via <c>FeatureRolloutEvaluator.IsInRollout</c> to decide whether a given
/// tenant is in this flag's rollout wave.
/// </param>
public sealed record FeatureFlagDto(
    string DatabaseKey,
    string FlagName,
    bool IsEnabled,
    string? Description,
    DateTime UpdatedUtc,
    string UpdatedBy,
    int? RolloutPercentage = null);
