// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           IFeatureFlagRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Repository abstraction over the real Phase 14 Governance
//                  [System].[FeatureFlag] tables (144-feature-flags.sql). Confirmed to
//                  exist ONLY in [PlantSafety.ArchiveDB] and [PlantSafety.IntegrationDB]
//                  (verified by reading the full migration script, not just its first two
//                  USE blocks). Each database's table is a fully independent, unrelated
//                  set of rows keyed by FlagName — there is no cross-database sync
//                  mechanism anywhere in the codebase, and the two seeded flags
//                  ("RetentionPurge.Enabled" in ArchiveDB, "ServiceBroker.
//                  ReplayProcessingEnabled" in IntegrationDB) are each meaningful only to
//                  their own database's consumers. This module surfaces the SAME table
//                  shape from both databases side by side for operator convenience; it
//                  does not imply the flags themselves are related or kept in sync.
//                  Implemented in the Infrastructure layer — NOT tenant-scoped, server-
//                  level platform data, so implementations connect directly by database
//                  key rather than going through ISqlConnectionFactory (same reasoning as
//                  Operations.SchemaGuard/Archival). Additive-only evolution.
//
//  SCOPE NOTE: "Thresholds", "Routing Rules", and "Tenant Overrides" were in-scope
//  candidates for this Configuration Framework module per the roadmap, but a full
//  search of PlantSafety.Infrastructure\Persistence\Migrations\**\*.sql for tables named
//  Threshold/RoutingRule/TenantOverride/TenantConfig (or similar) found NO backing SQL
//  Server tables for any of them as of this build (2026-07-20). Only unrelated
//  EnvironmentalHealth.Threshold *columns* exist, which are ordinary domain data, not a
//  configuration-editor target. Per the No-Guessing Rule for Schema Generation, no C#
//  DTOs/repositories were fabricated for these three areas. This module is therefore
//  scoped honestly to feature-flag management only until such tables are built.
// ============================================================================
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Repositories;

/// <summary>Repository over the real Phase 14 Governance [System].[FeatureFlag] tables.</summary>
public interface IFeatureFlagRepository
{
    /// <summary>Returns the logical database keys this repository knows how to reach (e.g. "ArchiveDB", "IntegrationDB").</summary>
    IReadOnlyList<string> GetDatabaseKeys();

    /// <summary>Returns every feature flag row from the given database's [System].[FeatureFlag] table, ordered by FlagName.</summary>
    /// <param name="databaseKey">The logical database key (must be one of <see cref="GetDatabaseKeys"/>).</param>
    Task<IReadOnlyList<FeatureFlagDto>> GetAllAsync(string databaseKey, CancellationToken cancellationToken);

    /// <summary>
    /// Returns every feature flag row from every known database (currently ArchiveDB and
    /// IntegrationDB), concatenated. Each row is independently tagged with its own
    /// <see cref="FeatureFlagDto.DatabaseKey"/> — this is a convenience aggregation for
    /// the operator UI, not evidence the flags are related across databases.
    /// </summary>
    Task<IReadOnlyList<FeatureFlagDto>> GetAllAcrossDatabasesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Toggles a single flag's IsEnabled value in the given database. A normal,
    /// reversible UPDATE — not a destructive action, so no additional guardrails beyond
    /// standard authentication/authorization are required. Sets UpdatedUtc and UpdatedBy
    /// (from the caller-supplied identity, not the SQL login) on every change.
    /// </summary>
    /// <param name="databaseKey">The logical database key the flag lives in.</param>
    /// <param name="flagName">The flag to toggle.</param>
    /// <param name="isEnabled">The new IsEnabled value.</param>
    /// <param name="updatedBy">The identity of the caller making the change (from ICurrentUserService), recorded in UpdatedBy.</param>
    Task<FeatureFlagDto> SetEnabledAsync(string databaseKey, string flagName, bool isEnabled, string updatedBy, CancellationToken cancellationToken);
}
