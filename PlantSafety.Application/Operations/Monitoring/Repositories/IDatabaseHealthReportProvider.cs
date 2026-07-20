// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Monitoring
//  Layer:          Application
//  File:           IDatabaseHealthReportProvider.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstraction over the ASP.NET Core
//                  Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService that
//                  already aggregates DatabaseHealthCheck / RedisHealthCheck /
//                  ServiceBusHealthCheck / SchemaVersionHealthCheck for the /health/ready
//                  probe (see ServiceCollectionExtensions.AddPlantSafetyHealthChecks).
//                  Deliberately does NOT re-implement any connectivity check itself — it
//                  only calls the already-registered HealthCheckService and projects the
//                  result into DatabaseHealthReportDto. Implemented in Infrastructure so
//                  the Application layer stays free of the ASP.NET Core hosting package
//                  dependency. Additive-only evolution.
// ============================================================================
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.Application.Operations.Monitoring.Models;

namespace PlantSafety.Application.Operations.Monitoring.Repositories;

/// <summary>Provides the aggregated /health/ready report as a DTO for the Monitoring dashboard.</summary>
public interface IDatabaseHealthReportProvider
{
    /// <summary>Runs every registered "ready"-tagged health check and returns the aggregated report.</summary>
    Task<DatabaseHealthReportDto> GetHealthReportAsync(CancellationToken cancellationToken);
}
