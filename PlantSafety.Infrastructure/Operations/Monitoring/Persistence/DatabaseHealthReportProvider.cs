// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Monitoring
//  Layer:          Infrastructure
//  File:           DatabaseHealthReportProvider.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Implementation of IDatabaseHealthReportProvider that calls the
//                  already-registered Microsoft.Extensions.Diagnostics.HealthChecks
//                  .HealthCheckService (see ServiceCollectionExtensions
//                  .AddPlantSafetyHealthChecks: database/redis/servicebus/schema-version,
//                  all tagged "ready") and projects the result into
//                  DatabaseHealthReportDto. Deliberately does not open any SqlConnection
//                  or re-implement any connectivity check itself. Additive-only evolution.
// ============================================================================
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using PlantSafety.Application.Operations.Monitoring.Models;
using PlantSafety.Application.Operations.Monitoring.Repositories;

namespace PlantSafety.Infrastructure.Operations.Monitoring.Persistence;

/// <summary>Wraps HealthCheckService to surface the /health/ready report as a DTO.</summary>
public sealed class DatabaseHealthReportProvider : IDatabaseHealthReportProvider
{
    private readonly HealthCheckService _healthCheckService;

    /// <summary>Initialises dependencies.</summary>
    public DatabaseHealthReportProvider(HealthCheckService healthCheckService) => _healthCheckService = healthCheckService;

    /// <inheritdoc/>
    public async Task<DatabaseHealthReportDto> GetHealthReportAsync(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(
            registration => registration.Tags.Contains("ready"),
            cancellationToken);

        var entries = report.Entries
            .Select(e => new HealthCheckEntryDto(
                e.Key,
                e.Value.Status.ToString(),
                e.Value.Description,
                e.Value.Duration.TotalMilliseconds,
                e.Value.Tags.ToList()))
            .ToList();

        return new DatabaseHealthReportDto(
            report.Status.ToString(),
            DateTime.UtcNow,
            report.TotalDuration.TotalMilliseconds,
            entries);
    }
}
