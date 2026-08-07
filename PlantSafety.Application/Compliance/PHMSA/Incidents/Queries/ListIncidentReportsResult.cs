// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           ListIncidentReportsResult.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Result payload for ListIncidentReportsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.PermitsQueryResult.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Incidents.Models;

namespace PlantSafety.Application.Compliance.PHMSA.Incidents.Queries;

/// <summary>Result of <see cref="ListIncidentReportsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching IncidentReport items.</param>
public sealed record ListIncidentReportsResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<IncidentReportDto> Items);
