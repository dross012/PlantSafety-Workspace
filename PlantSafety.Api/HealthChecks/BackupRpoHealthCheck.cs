// ============================================================================
// File: BackupRpoHealthCheck.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-03
// Description: Phase 6.6 (Enterprise Deployment & Release Architecture, §6.4 Disaster
//              Recovery) health check reporting current backup recency against the
//              formalized RpoRtoOptions target for each live PlantSafety.* database.
//              This is deliberately the ONLY new §6.4 piece -- it reuses the existing,
//              real GetBackupHealthQuery (Phase 12 + Phase 18 backup automation) rather
//              than duplicating any backup/restore logic; it only adds the enterprise
//              RPO-target comparison on top of that existing data.
// ============================================================================
//
// Onboarding Notes:
// - Registered in Extensions/ServiceCollectionExtensions.cs and mapped to the
//   /health/ready readiness probe, alongside DatabaseHealthCheck/RedisHealthCheck/etc.
// - Uses ISender (not a direct repository dependency) to reuse the existing
//   GetBackupHealthQuery/GetBackupHealthQueryHandler rather than re-implementing backup
//   recency logic here.
// - RpoRtoOptions.DefaultTier ("Critical" by default) is applied uniformly to every
//   database returned by GetBackupHealthQuery -- there is no per-database tier mapping
//   configured anywhere in this codebase today, so a uniform default is the honest,
//   non-fabricated choice until one is added.
// - Reports Degraded (not Unhealthy) when a database's last FULL backup is older than
//   the RPO target but the platform's own existing Green/Yellow/Red signal is not yet
//   Red, and Unhealthy when either the RPO target is breached AND the existing signal is
//   Red, or no FULL backup has ever completed for a database.
//
// ============================================================================

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using PlantSafety.Application.Operations.Backup.Queries;
using PlantSafety.Infrastructure.Operations.Backup.Options;

namespace PlantSafety.Api.HealthChecks;

/// <summary>
/// Health check reporting current backup recency against the formalized RPO target for
/// each live PlantSafety.* database.
/// </summary>
public sealed class BackupRpoHealthCheck : IHealthCheck
{
    private readonly ISender _sender;
    private readonly RpoRtoOptions _options;
    private readonly ILogger<BackupRpoHealthCheck> _logger;

    /// <summary>Initialises a new instance of <see cref="BackupRpoHealthCheck"/>.</summary>
    public BackupRpoHealthCheck(ISender sender, IOptions<RpoRtoOptions> options, ILogger<BackupRpoHealthCheck> logger)
    {
        _sender = sender;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Compares each live PlantSafety.* database's most recent successful FULL backup
    /// against the configured RPO target for <see cref="RpoRtoOptions.DefaultTier"/>.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _sender.Send(new GetBackupHealthQuery(), cancellationToken);

            if (result.IsFailure)
            {
                return HealthCheckResult.Unhealthy($"Unable to retrieve backup health: {result.Error.Message}");
            }

            var target = _options.ResolveTarget(_options.DefaultTier);
            var breaches = new List<string>();
            var noBackupEver = new List<string>();

            foreach (var db in result.Value)
            {
                if (db.LastFullBackupUtc is null)
                {
                    noBackupEver.Add(db.DatabaseName);
                    continue;
                }

                var age = DateTime.UtcNow - db.LastFullBackupUtc.Value;
                if (age > target.RecoveryPointObjective)
                {
                    breaches.Add($"{db.DatabaseName} (last FULL backup {age:g} ago, RPO target {target.RecoveryPointObjective:g})");
                }
            }

            var data = new Dictionary<string, object>
            {
                ["tier"] = target.Tier,
                ["rpoTarget"] = target.RecoveryPointObjective.ToString("g"),
                ["rtoTarget"] = target.RecoveryTimeObjective.ToString("g"),
                ["databasesEvaluated"] = result.Value.Count,
                ["rpoBreaches"] = breaches,
                ["neverBackedUp"] = noBackupEver
            };

            if (noBackupEver.Count > 0)
            {
                return HealthCheckResult.Unhealthy(
                    $"{noBackupEver.Count} database(s) have never completed a FULL backup: {string.Join(", ", noBackupEver)}.",
                    data: data);
            }

            if (breaches.Count > 0)
            {
                _logger.LogWarning("Backup RPO breach detected for tier {Tier}: {Breaches}", target.Tier, string.Join("; ", breaches));
                return HealthCheckResult.Degraded(
                    $"{breaches.Count} database(s) exceed the {target.Tier} tier's RPO target of {target.RecoveryPointObjective:g}: {string.Join(", ", breaches)}.",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"All {result.Value.Count} database(s) are within the {target.Tier} tier's RPO target of {target.RecoveryPointObjective:g}.",
                data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup RPO health check failed.");
            return HealthCheckResult.Unhealthy("Backup RPO health check failed.", ex);
        }
    }
}
