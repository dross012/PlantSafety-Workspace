// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Monitoring
//  Layer:          Application
//  File:           DatabaseHealthReportDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregated readiness report for the /health/ready probe, re-surfaced
//                  for the Operations.Monitoring dashboard. Wraps the same
//                  Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport already
//                  produced by DatabaseHealthCheck/RedisHealthCheck/ServiceBusHealthCheck/
//                  SchemaVersionHealthCheck (see PlantSafety.Api\HealthChecks and
//                  ServiceCollectionExtensions.AddPlantSafetyHealthChecks) rather than
//                  re-implementing any connectivity check here. Additive-only evolution.
// ============================================================================
using System;
using System.Collections.Generic;

namespace PlantSafety.Application.Operations.Monitoring.Models;

/// <summary>Aggregated snapshot of the platform's /health/ready readiness report.</summary>
/// <param name="OverallStatus">"Healthy" | "Degraded" | "Unhealthy" — mirrors HealthStatus.</param>
/// <param name="CheckedUtc">When the report was produced.</param>
/// <param name="TotalDurationMs">Total wall-clock time spent running all registered health checks.</param>
/// <param name="Entries">Per-check detail (database, redis, servicebus, schema-version, ...).</param>
public sealed record DatabaseHealthReportDto(
    string OverallStatus,
    DateTime CheckedUtc,
    double TotalDurationMs,
    IReadOnlyList<HealthCheckEntryDto> Entries);

/// <summary>A single named entry within <see cref="DatabaseHealthReportDto"/>.</summary>
/// <param name="Name">The registered health check name (e.g. "database", "schema-version").</param>
/// <param name="Status">"Healthy" | "Degraded" | "Unhealthy".</param>
/// <param name="Description">Human-readable detail returned by the check, if any.</param>
/// <param name="DurationMs">How long this individual check took to run.</param>
/// <param name="Tags">Tags the check was registered with (e.g. "ready").</param>
public sealed record HealthCheckEntryDto(
    string Name,
    string Status,
    string? Description,
    double DurationMs,
    IReadOnlyList<string> Tags);
