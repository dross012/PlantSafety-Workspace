// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Client-side DTOs for the Operations.Configuration module. Mirror
//   PlantSafety.Application.Operations.Configuration.Models.* returned by
//   PlantSafety.Api.Operations.Configuration.Controllers.V1
//   .OperationsConfigurationV1Controller. Surfaces the real Phase 14 Governance
//   [System].[FeatureFlag] tables (144-feature-flags.sql), confirmed present ONLY in
//   PlantSafety.ArchiveDB and PlantSafety.IntegrationDB.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.OperationsConfiguration.Models;

/// <summary>A single DB-level ops feature flag row from a specific database's [System].[FeatureFlag] table.</summary>
public sealed class FeatureFlagDto
{
    /// <summary>Gets the logical database key the flag lives in (e.g. "ArchiveDB", "IntegrationDB").</summary>
    public string DatabaseKey { get; init; } = string.Empty;

    /// <summary>Gets the unique flag name (e.g. "RetentionPurge.Enabled").</summary>
    public string FlagName { get; init; } = string.Empty;

    /// <summary>Gets whether the flag is currently enabled.</summary>
    public bool IsEnabled { get; init; }

    /// <summary>Gets a human-readable description of what the flag controls.</summary>
    public string? Description { get; init; }

    /// <summary>Gets when the flag was last changed.</summary>
    public DateTime UpdatedUtc { get; init; }

    /// <summary>Gets who last changed the flag.</summary>
    public string UpdatedBy { get; init; } = string.Empty;
}
