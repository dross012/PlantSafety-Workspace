// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Backup
//  Layer:          Infrastructure
//  File:           RpoRtoOptions.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 6.6 (Enterprise Deployment & Release Architecture, §6.4 Disaster
//                  Recovery) formalized, per-tier Recovery Point Objective / Recovery Time
//                  Objective targets. This is deliberately the ONLY new piece added for
//                  §6.4 -- the single-instance SQL Server backup/restore automation, job
//                  scheduling, and manual restore runbook already exist and are real, prior
//                  work (BackupOptions, BackupDapperRepository, Backup.sql,
//                  usp_Backup_GetBackupJobs.sql, Invoke-PlantSafetyBackup.ps1, DR-manual-
//                  restore-runbook.md -- see docs\Phase6\deployment-release-architecture.md).
//                  This type formalizes the enterprise/multi-region angle that was missing:
//                  explicit RPO/RTO targets per tier, consumed by BackupRpoHealthCheck to
//                  report current backup recency against the configured target. Bound from
//                  configuration section "Operations:Backup:RpoRto". No live multi-region
//                  failover exists in this sandboxed dev environment -- this is a
//                  configuration/reporting formalization, not a working failover mechanism.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Operations.Backup.Options;

/// <summary>
/// A single tier's Recovery Point Objective (maximum acceptable data loss window) and
/// Recovery Time Objective (maximum acceptable downtime) targets.
/// </summary>
/// <param name="Tier">The tier name (e.g. "Critical", "Standard").</param>
/// <param name="RecoveryPointObjective">
/// Maximum acceptable gap between the current time and the most recent successful backup.
/// </param>
/// <param name="RecoveryTimeObjective">
/// Maximum acceptable time to restore service after a declared disaster, for reporting/
/// documentation purposes -- not independently measured by any automated check in this
/// codebase (no real disaster-recovery drill infrastructure exists to measure it against).
/// </param>
public sealed record RpoRtoTarget(string Tier, TimeSpan RecoveryPointObjective, TimeSpan RecoveryTimeObjective);

/// <summary>Strongly typed configuration for formalized enterprise RPO/RTO targets.</summary>
public sealed class RpoRtoOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Operations:Backup:RpoRto";

    /// <summary>
    /// The tier every PlantSafety.* database is evaluated against by
    /// <see cref="PlantSafety.Api.HealthChecks.BackupRpoHealthCheck"/> when no per-database
    /// tier override is configured. Defaults to "Critical" -- the platform's databases carry
    /// safety-critical data, so the conservative default is the tighter target.
    /// </summary>
    public string DefaultTier { get; init; } = "Critical";

    /// <summary>
    /// The known RPO/RTO tiers. Defaults to two tiers matching the brief's example:
    /// Critical (1h RPO / 4h RTO) and Standard (24h RPO / 24h RTO). Operators may override
    /// via configuration to add tiers or change targets without a code change.
    /// </summary>
    public IReadOnlyList<RpoRtoTarget> Targets { get; init; } =
    [
        new("Critical", TimeSpan.FromHours(1), TimeSpan.FromHours(4)),
        new("Standard", TimeSpan.FromHours(24), TimeSpan.FromHours(24))
    ];

    /// <summary>Resolves the <see cref="RpoRtoTarget"/> for <paramref name="tier"/>, or <see cref="DefaultTier"/>'s target if not found.</summary>
    public RpoRtoTarget ResolveTarget(string? tier)
    {
        var lookupTier = string.IsNullOrWhiteSpace(tier) ? DefaultTier : tier;

        return Targets.FirstOrDefault(t => string.Equals(t.Tier, lookupTier, StringComparison.OrdinalIgnoreCase))
            ?? Targets.First(t => string.Equals(t.Tier, DefaultTier, StringComparison.OrdinalIgnoreCase));
    }
}
